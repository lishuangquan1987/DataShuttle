using CommunityToolkit.Mvvm.ComponentModel;

namespace DataShuttle.WpfSample.Configs.TransportConfigs
{
    public class WebSocketServerTransportConfig : ObservableObject
    {
        private int _bindingPort = 8080;
        public int BindingPort
        {
            get => _bindingPort;
            set => SetProperty(ref _bindingPort, value);
        }

        private string _path = "/ws";
        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }
    }
}
