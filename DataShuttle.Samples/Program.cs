// See https://aka.ms/new-console-template for more information
using DataShuttle;
using DataShuttle.Transports.SerialPort;

Console.WriteLine("Hello, World!");


var line = ShuttleLine.CreateBuilder()
    .AddFrom(SerialPortTransport.Create(new SerialPortTranportOptions()
    {
        PortName = "COM1"
    }))
    .AddTo(SerialPortTransport.Create(new SerialPortTranportOptions()
    {
        PortName = "COM2"
    }))
    .Build();

await line.Run();

Console.WriteLine("启动成功！");

Console.ReadLine();