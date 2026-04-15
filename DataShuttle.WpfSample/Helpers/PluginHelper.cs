using System.Collections.Generic;

namespace DataShuttle.WpfSample.Helpers
{
    public class PluginHelper
    {
        public static List<string> GetPluginNames() => new List<string>
        {
            TransportFactory.SerialPortName,
            TransportFactory.TcpClientName,
            TransportFactory.TcpServerName,
        };
    }
}
