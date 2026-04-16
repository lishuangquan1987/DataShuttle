using DataShuttle.Core.Helper;
using DataShuttle.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace DataShuttle.Transports.Udp
{
    public class UdpTransport : DataShuttle.Core.Interfaces.ITransport
    {
        private UdpSession _udpSession;
        private UdpTransportOptions _options;
        private ConcurrentQueue<byte[]> _buffer = new ConcurrentQueue<byte[]>();
        private CancellationTokenSource _readTokenSource = new CancellationTokenSource();
        private bool _isRunning;

        public UdpTransport() { }

        private UdpTransport(UdpTransportOptions options)
        {
            _options = options;
        }

        public static UdpTransport Create(UdpTransportOptions options) => new UdpTransport(options);

        public string Name => "UDP";
        public bool IsConnected => _isRunning;

        public event Action<bool> OnConnectionStatusChanged;
        public event Action<TransportErrorArgs> OnError;

        public async Task Run()
        {
            await Stop();

            _udpSession = new UdpSession();
            _udpSession.Received = UdpReceived;

            var config = new TouchSocketConfig()
                .SetBindIPHost(new IPHost(_options.BindingPort))
                .SetRemoteIPHost(new IPHost($"{_options.RemoteIp}:{_options.RemotePort}"));

            await _udpSession.SetupAsync(config);
            await _udpSession.StartAsync();

            _isRunning = true;
            OnConnectionStatusChanged?.Invoke(true);
        }

        private Task UdpReceived(IUdpSession client, UdpReceivedDataEventArgs e)
        {
            _buffer.Enqueue(e.Memory.ToArray());
            _readTokenSource.Cancel();
            return Task.CompletedTask;
        }

        public async Task<OperationResult<byte[]>> Read(CancellationToken token)
        {
            var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(token, _readTokenSource.Token).Token;
            await MethodHelper.Delay(linkedToken);

            if (token.IsCancellationRequested)
                return OperationResult<byte[]>.NG("取消读取");

            var result = new List<byte>();
            while (_buffer.TryDequeue(out byte[] data))
                result.AddRange(data);

            _readTokenSource = new CancellationTokenSource();
            return OperationResult<byte[]>.OK(result.ToArray());
        }

        public async Task<OperationResult> Write(byte[] data, CancellationToken token)
        {
            if (!_isRunning) return OperationResult.NG("未启动");
            try
            {
                await _udpSession.SendAsync(data);
                return OperationResult.OK();
            }
            catch (Exception e)
            {
                OnError?.Invoke(TransportErrorArgs.Create("Write", e.Message, e));
                return OperationResult.NG(e);
            }
        }

        public async Task Stop()
        {
            if (_udpSession != null)
            {
                _udpSession.Received = null;
                await _udpSession.StopAsync();
                _udpSession.Dispose();
                _udpSession = null;
            }
            _isRunning = false;
            OnConnectionStatusChanged?.Invoke(false);
        }

        public void Dispose() => _ = Stop();
    }
}
