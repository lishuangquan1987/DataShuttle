using DataShuttle.Core.Interfaces;
using DataShuttle.Transports.SerialPort;
using DataShuttle.Transports.TcpClient;
using DataShuttle.Transports.TcpServer;
using DataShuttle.Transports.Udp;
using DataShuttle.Transports.WebSocket;
using DataShuttle.WpfSample.Configs;

namespace DataShuttle.WpfSample.Helpers
{
    public static class TransportFactory
    {
        public const string SerialPortName = "串口";
        public const string TcpClientName = "TCP客户端";
        public const string TcpServerName = "TCP服务端";
        public const string UdpName = "UDP";
        public const string WebSocketClientName = "WebSocket客户端";
        public const string WebSocketServerName = "WebSocket服务端";

        public static ITransport Create(TransportConfig config)
        {
            switch (config.Type)
            {
                case SerialPortName:
                    return SerialPortTransport.Create(new SerialPortTransportOptions
                    {
                        PortName = config.SerialPortTransportConfig.PortName,
                        BauRate = config.SerialPortTransportConfig.BauRate,
                        Parity = config.SerialPortTransportConfig.Parity,
                        DataBits = config.SerialPortTransportConfig.DataBits,
                        StopBits = config.SerialPortTransportConfig.StopBits,
                    });
                case TcpClientName:
                    return TcpClientTransport.Create(new TcpClientTransportOptions
                    {
                        ServerIp = config.TcpClientTransportConfig.ServerIp,
                        ServerPort = config.TcpClientTransportConfig.ServerPort,
                    });
                case TcpServerName:
                    return TcpServerTransport.Create(new TcpServerTransportOptions
                    {
                        BindingIp = config.TcpServerTransportConfig.BindingIp,
                        BindingPort = config.TcpServerTransportConfig.BindingPort,
                    });
                case UdpName:
                    return UdpTransport.Create(new UdpTransportOptions
                    {
                        BindingPort = config.UdpTransportConfig.BindingPort,
                        RemoteIp = config.UdpTransportConfig.RemoteIp,
                        RemotePort = config.UdpTransportConfig.RemotePort,
                    });
                case WebSocketClientName:
                    return WebSocketClientTransport.Create(new WebSocketClientTransportOptions
                    {
                        Url = config.WebSocketClientTransportConfig.Url,
                    });
                case WebSocketServerName:
                    return WebSocketServerTransport.Create(new WebSocketServerTransportOptions
                    {
                        BindingPort = config.WebSocketServerTransportConfig.BindingPort,
                        Path = config.WebSocketServerTransportConfig.Path,
                    });
                default:
                    return null;
            }
        }

        public static string GetSummary(TransportConfig config)
        {
            switch (config.Type)
            {
                case SerialPortName:
                    return $"{config.SerialPortTransportConfig.PortName} {config.SerialPortTransportConfig.BauRate}";
                case TcpClientName:
                    return $"{config.TcpClientTransportConfig.ServerIp}:{config.TcpClientTransportConfig.ServerPort}";
                case TcpServerName:
                    return $"{config.TcpServerTransportConfig.BindingIp}:{config.TcpServerTransportConfig.BindingPort}";
                case UdpName:
                    return $"本地:{config.UdpTransportConfig.BindingPort} → {config.UdpTransportConfig.RemoteIp}:{config.UdpTransportConfig.RemotePort}";
                case WebSocketClientName:
                    return config.WebSocketClientTransportConfig.Url;
                case WebSocketServerName:
                    return $":{config.WebSocketServerTransportConfig.BindingPort}{config.WebSocketServerTransportConfig.Path}";
                default:
                    return config.Type ?? "未配置";
            }
        }
    }
}
