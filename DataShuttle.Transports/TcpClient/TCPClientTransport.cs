using DataShuttle.Core.Helper;
using DataShuttle.Core.Interfaces;
using DataShuttle.Core.Models;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace DataShuttle.Transports.TcpClient
{
    public class TCPClientTransport : DataShuttle.Core.Interfaces.ITransport
    {
        private TouchSocket.Sockets.TcpClient _tcpClient;
        private TCPClientTransportOptions _options;
        private CancellationTokenSource? _startTokenSource;
        public bool IsConnected => throw new NotImplementedException();

        public bool IsError => throw new NotImplementedException();

        public string? ErrorMsg => throw new NotImplementedException();

        public event Action<bool> OnConnectionStatusChanged;
        public event Action<bool, string> OnErrorStatusChanged;

        private TCPClientTransport(TCPClientTransportOptions options)
        {
            this._options = options;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<byte[]>> Read(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public async Task Run()
        {
            await Stop();

            _startTokenSource = new CancellationTokenSource();

            _tcpClient = new TouchSocket.Sockets.TcpClient();
            _tcpClient.conn += TcpClientConnected;
            _tcpClient.Received += TcpClientReceived;
            await _tcpClient.SetupAsync(new TouchSocket.Core.TouchSocketConfig()
                .SetRemoteIPHost($"{_options.ServerIp}:{_options.ServerPort}")
                .ConfigurePlugins(a =>
                {
                    a.UseReconnection<TouchSocket.Sockets.TcpClient>(op => op.PollingInterval = TimeSpan.FromSeconds(1));
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
                    }
                }
            });

        }

        private async Task TcpClientReceived(TouchSocket.Sockets.ITcpClient client, TouchSocket.Sockets.ReceivedDataEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async Task TcpClientConnected(TouchSocket.Sockets.ITcpClient client, TouchSocket.Sockets.ConnectedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public async Task Stop()
        {
            _startTokenSource?.Cancel();
            if (_tcpClient != null)
            {
                await _tcpClient.CloseAsync("主动关闭");
                _tcpClient.Connected -= TcpClientConnected;
                _tcpClient.Received -= TcpClientReceived;
            }
        }

        public Task<OperationResult> Write(byte[] data, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
