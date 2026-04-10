using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShuttle.WpfSample.Configs.TransportConfigs
{
    public class TcpServerTransportConfig : ObservableObject
    {
        private string _bindingIp = "127.0.0.1";
        public string BindingIp
        {
            get => _bindingIp;
            set => SetProperty(ref _bindingIp, value);
        }

        private int _bindingPort = 777;
        public int BindingPort
        {
            get => _bindingPort;
            set => SetProperty(ref _bindingPort, value);
        }
    }
}
