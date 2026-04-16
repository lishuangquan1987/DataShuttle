using DataShuttle.Core.Helper;
using DataShuttle.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http.WebSockets;
using TouchSocket.Sockets;

namespace DataShuttle.Transports.WebSocket
{
    public class WebSocketClientTransport : DataShuttle.Core.Interfaces.ITransport
    {
        private TouchSocket.Http.WebSockets.WebSocketClient _wsClient;
        private WebSocketClientTransportOptions _options;
        private CancellationTokenSource _startTokenSource;
        private ConcurrentQueue<byte[]> _buffer = new ConcurrentQueue<byte[]>();
        private CancellationTokenSource _readTokenSource = new CancellationTokenSource();
        private bool _isConnected;

        public WebSocketClientTransport() { }

        private WebSocketClientTransport(WebSocketClientTransportOptions options)
        {
            _options = options;
        }

        public static WebSocketClientTransport Create(WebSocketClientTransportOptions options) =>
            new WebSocketClientTransport(options);

        public string Name => "WebSocket客户端";
        public bool IsConnected => _isConnected;

        public event Action<bool> OnConnectionStatusChanged;
        public event Action<TransportErrorArgs> OnError;

        public async Task Run()
        {
            await Stop();

            _startTokenSource = new CancellationTokenSource();
            _wsClient = new TouchSocket.Http.WebSockets.WebSocketClient();
            _wsClient.Received = WsReceived;
            _wsClient.Closed = WsClosed;

            await _wsClient.SetupAsync(new TouchSocketConfig()
                .SetRemoteIPHost(_options.Url)
                .ConfigurePlugins(a =>
                {
                    a.UseReconnection<TouchSocket.Http.WebSockets.WebSocketClient>(op =>
                        op.PollingInterval = TimeSpan.FromSeconds(1));
                }));

            _ = Task.Factory.StartNew(async () =>
            {
                var token = _startTokenSource.Token;
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await _wsClient.ConnectAsync(token);
                        SetConnected(true);
                        break;
                    }
                    catch (Exception ex)
                    {
                        await MethodHelper.Delay(TimeSpan.FromSeconds(3), token);
                        OnError?.Invoke(TransportErrorArgs.Create("Connect", ex.Message, ex));
                    }
                }
            });
        }

        private Task WsReceived(IWebSocketClient client, WSDataFrameEventArgs e)
        {
            if (e.DataFrame.Opcode == WSDataType.Binary || e.DataFrame.Opcode == WSDataType.Text)
            {
                _buffer.Enqueue(e.DataFrame.PayloadData.ToArray());
                _readTokenSource.Cancel();
            }
            return Task.CompletedTask;
        }

        private Task WsClosed(IWebSocketClient client, ClosedEventArgs e)
        {
            SetConnected(false);
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
            if (!_isConnected) return OperationResult.NG("未连接");
            try
            {
                await _wsClient.SendAsync(data);
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
            _startTokenSource?.Cancel();
            if (_wsClient != null)
            {
                _wsClient.Received = null;
                _wsClient.Closed = null;
                await _wsClient.CloseAsync("主动关闭");
                _wsClient.Dispose();
                _wsClient = null;
            }
        }

        public void Dispose() => _ = Stop();

        private void SetConnected(bool connected)
        {
            if (_isConnected == connected) return;
            _isConnected = connected;
            OnConnectionStatusChanged?.Invoke(_isConnected);
        }
    }
}
