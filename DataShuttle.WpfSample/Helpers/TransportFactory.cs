using DataShuttle.Core.Interfaces;
using DataShuttle.Transports.SerialPort;
using DataShuttle.Transports.TcpClient;
using DataShuttle.Transports.TcpServer;
using DataShuttle.WpfSample.Configs;

namespace DataShuttle.WpfSample.Helpers
{
    public static class TransportFactory
    {
        public const string SerialPortName = "串口";
        public const string TcpClientName = "TCP客户端";
        public const string TcpServerName = "TCP服务端";

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
                default:
                    return config.Type ?? "未配置";
            }
        }
    }
}
