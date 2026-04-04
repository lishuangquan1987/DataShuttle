using DataShuttle.Core.Interfaces;
using DataShuttle.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShuttle.Transports
{
    public class SerialPortTransport : ITransport
    {
        private System.IO.Ports.SerialPort _serialPort=new System.IO.Ports.SerialPort();
        private bool _isConnected = false;
        private string? _errMsg;
        public bool IsConnected => _isConnected;

        public string? ErrorMsg => _errMsg;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<OperationResult<byte[]>> Read(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task Start()
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
