using CommunityToolkit.Mvvm.ComponentModel;

namespace DataShuttle.WpfSample.Configs.TransportConfigs
{
    public class UdpTransportConfig : ObservableObject
    {
        private int _bindingPort = 5000;
        public int BindingPort
        {
            get => _bindingPort;
            set => SetProperty(ref _bindingPort, value);
        }

        private string _remoteIp = "127.0.0.1";
        public string RemoteIp
        {
            get => _remoteIp;
            set => SetProperty(ref _remoteIp, value);
        }

        private int _remotePort = 5001;
        public int RemotePort
        {
            get => _remotePort;
            set => SetProperty(ref _remotePort, value);
        }
    }
}
