using DataShuttle.Core.Helper;
using DataShuttle.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
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
        private bool _isError;
        private string _errMsg;
        private TcpService _tcpService;
        private TcpServerTransportOptions _options;
        private ConcurrentQueue<byte[]> buffer = new ConcurrentQueue<byte[]>();

        private TcpServerTransport(TcpServerTransportOptions options)
        {
            this._options = options;
        }

        public static TcpServerTransport Create(TcpServerTransportOptions options) => new TcpServerTransport(options);

        public string Name => "TCP服务端";

        public bool IsConnected => _tcpService != null &&
            _tcpService.ServerState == ServerState.Running &&
            _tcpService.Clients.Count > 0;

        public bool IsError => _isError;

        public string ErrorMsg => _errMsg;

        public event Action<bool> OnConnectionStatusChanged;
        public event Action<bool, string> OnErrorStatusChanged;

        public void Dispose()
        {
            _ = Stop();
        }

        private void RaiseErrorChanged(bool isError, string errMsg)
        {
            if (!_isError && !isError) return;

            this.OnErrorStatusChanged?.Invoke(isError, errMsg);
        }

        public async Task<OperationResult<byte[]>> Read(CancellationToken token)
        {
            CancellationToken newToken = CancellationTokenSource.CreateLinkedTokenSource(token, _readTokenSource.Token).Token;
            await MethodHelper.Delay(newToken);
            if (token.IsCancellationRequested)
            {
                return OperationResult<byte[]>.NG("取消读取");
            }

            List<byte> result = new List<byte>();
            while (buffer.TryDequeue(out byte[] data))
            {
                result.AddRange(data);
            }
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
                    await _tcpService.SetupAsync(new TouchSocket.Core.TouchSocketConfig()
            .SetListenIPHosts($"tcp://{_options.BindingIp}:{_options.BindingPort}"));
                    await _tcpService.StartAsync(_startTokenSource.Token);
                    break;
                }
                catch (Exception e)
                {
                    await MethodHelper.Delay(TimeSpan.FromSeconds(3), _startTokenSource.Token);
                    RaiseErrorChanged(true, e.Message);
                }
            }

        }

        private async Task TcpServiceReceived(TcpSessionClient client, ReceivedDataEventArgs e)
        {
            buffer.Enqueue(e.Memory.Span.ToArray());
            _readTokenSource.Cancel();
        }

        private async Task TcpServiceClosed(TcpSessionClient client, ClosedEventArgs e)
        {
            this.OnConnectionStatusChanged?.Invoke(IsConnected);
        }

        private async Task TcpServiceConnected(TcpSessionClient client, ConnectedEventArgs e)
        {
            this.OnConnectionStatusChanged?.Invoke(IsConnected);
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
                foreach (var clinet in _tcpService.Clients)
                {
                    await clinet.SendAsync(data, token);
                }
                return OperationResult.OK();
            }
            catch (Exception e)
            {
                RaiseErrorChanged(true, e.Message);
                return OperationResult.NG(e);
            }
        }
    }
}
