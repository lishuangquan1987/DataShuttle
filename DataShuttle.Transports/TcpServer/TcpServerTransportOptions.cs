using System;
using System.Collections.Generic;
using System.Text;

namespace DataShuttle.Transports.TcpServer
{
    public class TcpServerTransportOptions
    {
        public string BindingIp { get; set; }
        public string BindingPort { get; set; }
    }
}
