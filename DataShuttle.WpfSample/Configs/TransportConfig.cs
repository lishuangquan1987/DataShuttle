using CommunityToolkit.Mvvm.ComponentModel;
using DataShuttle.Transports.SerialPort;
using DataShuttle.WpfSample.Configs.TransportConfigs;

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
        public UdpTransportConfig UdpTransportConfig { get; set; } = new UdpTransportConfig();
        public WebSocketClientTransportConfig WebSocketClientTransportConfig { get; set; } = new WebSocketClientTransportConfig();
        public WebSocketServerTransportConfig WebSocketServerTransportConfig { get; set; } = new WebSocketServerTransportConfig();
    }
}
