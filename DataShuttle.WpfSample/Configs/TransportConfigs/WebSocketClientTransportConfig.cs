using CommunityToolkit.Mvvm.ComponentModel;

namespace DataShuttle.WpfSample.Configs.TransportConfigs
{
    public class WebSocketClientTransportConfig : ObservableObject
    {
        private string _url = "ws://127.0.0.1:8080/ws";
        public string Url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }
    }
}
