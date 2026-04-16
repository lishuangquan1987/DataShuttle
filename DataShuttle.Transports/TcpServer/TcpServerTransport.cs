using DataShuttle.Core.Helper;
using DataShuttle.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace DataShuttle.Transports.TcpServer
{
    public class TcpServerTransport : DataShuttle.Core.Interfaces.ITransport
    {
        private CancellationTokenSource _startTokenSource = new CancellationTokenSource();
        private CancellationTokenSource _readTokenSource = new CancellationTokenSource();
        private TcpService _tcpService;
        private TcpServerTransportOptions _options;
        private ConcurrentQueue<byte[]> _buffer = new ConcurrentQueue<byte[]>();

        public TcpServerTransport() { }

        private TcpServerTransport(TcpServerTransportOptions options)
        {
            _options = options;
        }

        public static TcpServerTransport Create(TcpServerTransportOptions options) => new TcpServerTransport(options);

        public string Name => "TCP服务端";

        public bool IsConnected => _tcpService != null &&
            _tcpService.ServerState == ServerState.Running &&
            _tcpService.Clients.Count > 0;

        public event Action<bool> OnConnectionStatusChanged;
        public event Action<TransportErrorArgs> OnError;

        public void Dispose() => _ = Stop();

        public async Task<OperationResult<byte[]>> Read(CancellationToken token)
        {
            var newToken = CancellationTokenSource.CreateLinkedTokenSource(token, _readTokenSource.Token).Token;
            await MethodHelper.Delay(newToken);

            if (token.IsCancellationRequested)
                return OperationResult<byte[]>.NG("取消读取");

            var result = new List<byte>();
            while (_buffer.TryDequeue(out byte[] data))
                result.AddRange(data);

            _readTokenSource = new CancellationTokenSource();
            return OperationResult<byte[]>.OK(result.ToArray());
        }

        public async Task Run()
        {
            await Stop();

            _startTokenSource = new CancellationTokenSource();
            _tcpService = new TcpService();
            _tcpService.Connected += TcpServiceConnected;
            _tcpService.Closed += TcpServiceClosed;
            _tcpService.Received += TcpServiceReceived;

            while (!_startTokenSource.IsCancellationRequested)
            {
                try
                {
                    await _tcpService.SetupAsync(new TouchSocketConfig()
                        .SetListenIPHosts($"tcp://{_options.BindingIp}:{_options.BindingPort}"));
                    await _tcpService.StartAsync(_startTokenSource.Token);
                    break;
                }
                catch (Exception e)
                {
                    await MethodHelper.Delay(TimeSpan.FromSeconds(3), _startTokenSource.Token);
                    OnError?.Invoke(TransportErrorArgs.Create("Connect", e.Message, e));
                }
            }
        }

        private async Task TcpServiceReceived(TcpSessionClient client, ReceivedDataEventArgs e)
        {
            _buffer.Enqueue(e.Memory.Span.ToArray());
            _readTokenSource.Cancel();
        }

        private async Task TcpServiceClosed(TcpSessionClient client, ClosedEventArgs e)
        {
            OnConnectionStatusChanged?.Invoke(IsConnected);
        }

        private async Task TcpServiceConnected(TcpSessionClient client, ConnectedEventArgs e)
        {
            OnConnectionStatusChanged?.Invoke(IsConnected);
        }

        public async Task Stop()
        {
            _startTokenSource?.Cancel();
            if (_tcpService != null && _tcpService.ServerState == ServerState.Running)
            {
                await _tcpService.StopAsync();
                _tcpService.Connected -= TcpServiceConnected;
                _tcpService.Closed -= TcpServiceClosed;
                _tcpService.Received -= TcpServiceReceived;
                _tcpService.Dispose();
            }
        }

        public async Task<OperationResult> Write(byte[] data, CancellationToken token)
        {
            if (!IsConnected) return OperationResult.NG("未连接");
            try
            {
                foreach (var client in _tcpService.Clients)
                    await client.SendAsync(data, token);
                return OperationResult.OK();
            }
            catch (Exception e)
            {
                OnError?.Invoke(TransportErrorArgs.Create("Write", e.Message, e));
                return OperationResult.NG(e);
            }
        }
    }
}
