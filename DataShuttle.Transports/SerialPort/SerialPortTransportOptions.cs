using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShuttle.Transports.SerialPort
{
    public class SerialPortTransportOptions
    {
        public string PortName { get; set; } = "COM1";
        public int BauRate { get; set; } = 9600;
        public int Parity { get; set; } = (int)System.IO.Ports.Parity.None;
        public int DataBits { get; set; } = 8;
        public int StopBits { get; set; } = (int)System.IO.Ports.StopBits.One;

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
