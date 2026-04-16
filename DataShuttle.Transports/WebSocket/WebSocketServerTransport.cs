using DataShuttle.Core.Helper;
using DataShuttle.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Http.WebSockets;
using TouchSocket.Sockets;

namespace DataShuttle.Transports.WebSocket
{
    public class WebSocketServerTransport : DataShuttle.Core.Interfaces.ITransport
    {
        private HttpService _httpService;
        private WebSocketServerTransportOptions _options;
        private CancellationTokenSource _startTokenSource;
        private ConcurrentQueue<byte[]> _buffer = new ConcurrentQueue<byte[]>();
        private CancellationTokenSource _readTokenSource = new CancellationTokenSource();

        public WebSocketServerTransport() { }

        private WebSocketServerTransport(WebSocketServerTransportOptions options)
        {
            _options = options;
        }

        public static WebSocketServerTransport Create(WebSocketServerTransportOptions options) =>
            new WebSocketServerTransport(options);

        public string Name => "WebSocket服务端";

        public bool IsConnected => _httpService != null &&
            _httpService.ServerState == ServerState.Running &&
            _httpService.Clients.Count > 0;

        public event Action<bool> OnConnectionStatusChanged;
        public event Action<TransportErrorArgs> OnError;

        public async Task Run()
        {
            await Stop();

            _startTokenSource = new CancellationTokenSource();
            _httpService = new HttpService();

            while (!_startTokenSource.IsCancellationRequested)
            {
                try
                {
                    await _httpService.SetupAsync(new TouchSocketConfig()
                        .SetListenIPHosts(_options.BindingPort)
                        .ConfigurePlugins(a =>
                        {
                            a.UseWebSocket(_options.Path);
                            a.Add<WsReceiverPlugin>().SetBuffer(_buffer, _readTokenSource, OnConnectionStatusChanged);
                        }));

                    await _httpService.StartAsync(_startTokenSource.Token);
                    break;
                }
                catch (Exception e)
                {
                    await MethodHelper.Delay(TimeSpan.FromSeconds(3), _startTokenSource.Token);
                    OnError?.Invoke(TransportErrorArgs.Create("Connect", e.Message, e));
                }
            }
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
            if (!IsConnected) return OperationResult.NG("无客户端连接");
            try
            {
                foreach (var client in _httpService.Clients)
                {
                    if (client.WebSocket != null && client.WebSocket.Online)
                        await client.WebSocket.SendAsync(data);
                }
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
            if (_httpService != null && _httpService.ServerState == ServerState.Running)
            {
                await _httpService.StopAsync();
                _httpService.Dispose();
            }
            _httpService = null;
        }

        public void Dispose() => _ = Stop();
    }

    internal class WsReceiverPlugin : PluginBase, IWebSocketReceivedPlugin, IWebSocketConnectedPlugin, IWebSocketClosedPlugin
    {
        private ConcurrentQueue<byte[]> _buffer;
        private CancellationTokenSource _readTokenSource;
        private Action<bool> _onConnectionStatusChanged;

        public WsReceiverPlugin SetBuffer(ConcurrentQueue<byte[]> buffer,
            CancellationTokenSource readTokenSource,
            Action<bool> onConnectionStatusChanged)
        {
            _buffer = buffer;
            _readTokenSource = readTokenSource;
            _onConnectionStatusChanged = onConnectionStatusChanged;
            return this;
        }

        public async Task OnWebSocketReceived(IWebSocket client, WSDataFrameEventArgs e)
        {
            if (e.DataFrame.Opcode == WSDataType.Binary || e.DataFrame.Opcode == WSDataType.Text)
            {
                _buffer.Enqueue(e.DataFrame.PayloadData.ToArray());
                _readTokenSource.Cancel();
            }
            await e.InvokeNext();
        }

        public async Task OnWebSocketConnected(IWebSocket client, HttpContextEventArgs e)
        {
            _onConnectionStatusChanged?.Invoke(true);
            await e.InvokeNext();
        }

        public async Task OnWebSocketClosed(IWebSocket client, ClosedEventArgs e)
        {
            _onConnectionStatusChanged?.Invoke(false);
            await e.InvokeNext();
        }
    }
}
