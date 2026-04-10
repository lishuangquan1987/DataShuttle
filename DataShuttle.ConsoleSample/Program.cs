using DataShuttle;
using DataShuttle.Transports.SerialPort;
using DataShuttle.Transports.TcpClient;
using System;

public class Program
{
    public static async void Main1(string[] args)
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

    public static void Main(string[] args)
    {
        var line = ShuttleLine.CreateBuilder()
            .AddFrom(TcpClientTransport.Create(new TcpClientTransportOptions()
            {
                ServerIp = "127.0.0.1",
                ServerPort = 777
            }))
            .AddTo(TcpClientTransport.Create(new TcpClientTransportOptions()
            {
                ServerIp = "127.0.0.1",
                ServerPort = 888
            }))
            .Build();
        line.Run().GetAwaiter().GetResult();
        Console.WriteLine("启动成功！");
        Console.ReadLine();
    }
}