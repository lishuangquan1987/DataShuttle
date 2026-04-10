using DataShuttle.Core.Helper;
using DataShuttle.Core.Interfaces;
using DataShuttle.Core.Models;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace DataShuttle.Transports.TcpClient
{
    public class TcpClientTransport : DataShuttle.Core.Interfaces.ITransport
    {
        private TouchSocket.Sockets.TcpClient _tcpClient;
        private TcpClientTransportOptions _options;
        private CancellationTokenSource _startTokenSource;
        private IReceiver<IReceiverResult> _receiver = null;
        private bool _isConnected;
        private bool _isError;
        private string _errMsg;
        public bool IsConnected => _isConnected;

        public bool IsError => _isError;

        public string ErrorMsg => _errMsg;
        public string Name => "TCP客户端";

        public event Action<bool> OnConnectionStatusChanged;
        public event Action<bool, string> OnErrorStatusChanged;

        private TcpClientTransport(TcpClientTransportOptions options)
        {
            this._options = options;
        }

        public static TcpClientTransport Create(TcpClientTransportOptions options) => new TcpClientTransport(options);

        public void Dispose()
        {
            _ = Stop();
        }

        public async Task<OperationResult<byte[]>> Read(CancellationToken token)
        {
            if (_receiver != null && IsConnected)
            {
                using (var result = await _receiver.ReadAsync(token))
                {
                    if (result.IsCompleted) return OperationResult<byte[]>.NG("断开了连接");

                    return OperationResult<byte[]>.OK(result.Memory.Span.ToArray());
                }
            }

            return OperationResult<byte[]>.NG("未连接");
        }

        public async Task Run()
        {
            await Stop();

            _startTokenSource = new CancellationTokenSource();

            _tcpClient = new TouchSocket.Sockets.TcpClient();
            _tcpClient.Connected += TcpClientConnected;
            _tcpClient.Closed += TcpClientClosed;
            //_tcpClient.Received += TcpClientReceived;
            await _tcpClient.SetupAsync(new TouchSocket.Core.TouchSocketConfig()
                .SetRemoteIPHost($"{_options.ServerIp}:{_options.ServerPort}")
                .ConfigurePlugins(a =>
                {
                    a.UseReconnection<TouchSocket.Sockets.TcpClient>(op =>
                        op.PollingInterval = TimeSpan.FromSeconds(1));
                }));

            _ = Task.Factory.StartNew(async () =>
            {
                var token = _startTokenSource.Token;
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await _tcpClient.ConnectAsync(_startTokenSource.Token);
                        break;
                    }
                    catch (Exception ex)
                    {
                        await MethodHelper.Delay(TimeSpan.FromSeconds(3), token);
                        SetError(true, ex.Message);
                    }
                }
            });
        }

        private void SetError(bool isError, string errorMsg = null)
        {
            if (!_isError && !isError) return;

            _errMsg = errorMsg;
            _isError = isError;
            this.OnErrorStatusChanged?.Invoke(_isError, _errMsg);
        }

        private void SetConnectionStatus(bool isConnected)
        {
            if (_isConnected == isConnected) return;

            _isConnected = isConnected;
            this.OnConnectionStatusChanged?.Invoke(_isConnected);
        }


        private async Task TcpClientClosed(ITcpClient client, ClosedEventArgs e)
        {
            _receiver?.Dispose();
            SetConnectionStatus(false);
        }

        private async Task TcpClientConnected(TouchSocket.Sockets.ITcpClient client,
            TouchSocket.Sockets.ConnectedEventArgs e)
        {
            _receiver = client.CreateReceiver();
            SetConnectionStatus(true);
        }

        public async Task Stop()
        {
            _startTokenSource?.Cancel();
            if (_tcpClient != null)
            {
                _tcpClient.ClearReceiver();
                if (_receiver != null)
                {
                    _receiver.Dispose();
                    _receiver = null;
                }

                await _tcpClient.CloseAsync("主动关闭");
                _tcpClient.Connected -= TcpClientConnected;
                _tcpClient.Closed -= TcpClientClosed;
            }
        }

        public async Task<OperationResult> Write(byte[] data, CancellationToken token)
        {
            try
            {
                if (_tcpClient != null)
                {
                    await _tcpClient.SendAsync(data, token);
                    return OperationResult.OK();
                }

                return OperationResult.NG("未连接");
            }
            catch (Exception e)
            {
                SetError(true, e.Message);
                return OperationResult.NG(e);
            }
        }
    }
}