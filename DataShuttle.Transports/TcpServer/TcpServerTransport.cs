using DataShuttle.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Sockets;

namespace DataShuttle.Transports.TcpServer
{
    public class TcpServerTransport : DataShuttle.Core.Interfaces.ITransport
    {
        private CancellationTokenSource _startTokenSource;
        private bool _isConnected;
        private bool _isError;
        private string _errMsg;
        public string Name => "TCP服务端";

        public bool IsConnected => _isConnected;

        public bool IsError => _isError;

        public string ErrorMsg => _errMsg;

        public event Action<bool> OnConnectionStatusChanged;
        public event Action<bool, string> OnErrorStatusChanged;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<byte[]>> Read(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Run()
        {
            throw new NotImplementedException();
        }

        public Task Stop()
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult> Write(byte[] data, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
