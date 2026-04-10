using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShuttle.WpfSample.Configs.TransportConfigs
{
    public class TcpClientTransportConfig : ObservableObject
    {
        private string serverIp = "127.0.0.1";
        public string ServerIp
        {
            get => serverIp;
            set => SetProperty(ref serverIp, value);
        }

        private int serverPort = 777;
        public int ServerPort
        {
            get => serverPort;
            set => SetProperty(ref serverPort, value);
        }
    }
}
