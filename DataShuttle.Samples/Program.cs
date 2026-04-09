// See https://aka.ms/new-console-template for more information
using DataShuttle;
using DataShuttle.Transports.SerialPort;
using DataShuttle.Transports.TcpClient;

public class Program
{
    public static async Task Main1(string[] args)
    {
        var line = ShuttleLine.CreateBuilder()
            .AddFrom(SerialPortTransport.Create(new SerialPortTransportOptions()
            {
                PortName = "COM11"
            }))
            .AddTo(SerialPortTransport.Create(new SerialPortTransportOptions()
            {
                PortName = "COM21"
            }))
            .Build();
        await line.Run();
        Console.WriteLine("启动成功！");
        Console.ReadLine();
    }

    public static async Task Main(string[] args)
    {
        var line = ShuttleLine.CreateBuilder()
            .AddFrom(TCPClientTransport.Create(new TCPClientTransportOptions()
            {
                ServerIp = "127.0.0.1",
                ServerPort = 777
            }))
            .AddTo(TCPClientTransport.Create(new TCPClientTransportOptions()
            {
                ServerIp = "127.0.0.1",
                ServerPort = 888
            }))
            .Build();
        await line.Run();
        Console.WriteLine("启动成功！");
        Console.ReadLine();
    }
}