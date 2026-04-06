using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShuttle.Transports
{
    public class SerialPortTransportBuilder
    {
        private SerialPortTransport _serialPortTransport;

        public SerialPortTransportBuilder SetPortName(string portName)
        {
            this._serialPortTransport._serialPort.PortName=portName;
            return this;
        }

        public SerialPortTransportBuilder SetBauRate(int bauRate) 
        {
            this._serialPortTransport._serialPort.BaudRate=bauRate;
            return this;
        }

        public SerialPortTransportBuilder SetStopBits(int stopBits)
        {
            this._serialPortTransport._serialPort.StopBits = (StopBits)stopBits;
            return this;
        }
    }
}
