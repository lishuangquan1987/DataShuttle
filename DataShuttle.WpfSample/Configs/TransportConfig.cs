using CommunityToolkit.Mvvm.ComponentModel;
using DataShuttle.Transports.SerialPort;
using DataShuttle.WpfSample.Configs.TransportConfigs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShuttle.WpfSample.Configs
{
    public class TransportConfig : ObservableObject
    {
        private string _type;
        public string Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public SerialPortTransportConfig SerialPortTransportConfig { get; set; } = new SerialPortTransportConfig();
        public TcpServerTransportConfig TcpServerTransportConfig { get; set; } = new TcpServerTransportConfig();
        public TcpClientTransportConfig TcpClientTransportConfig { get; set; } = new TcpClientTransportConfig();
    }
}
