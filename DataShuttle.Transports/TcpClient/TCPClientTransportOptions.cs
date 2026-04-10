using System;
using System.Collections.Generic;
using System.Text;

namespace DataShuttle.Transports.TcpClient
{
    public class TcpClientTransportOptions
    {
        public string ServerIp { get; set; }
        public int ServerPort { get; set; }
    }
}
