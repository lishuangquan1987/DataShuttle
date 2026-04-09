# DataShuttle

DataShuttle 是一个轻量级的数据传输库，用于在不同的传输介质之间无缝传输数据，支持串口(SerialPort)、TCP客户端(TCPClient)和TCP服务器(TcpServer)之间的数据转发。

## 功能特点

- **多传输介质支持**：支持串口、TCP客户端和TCP服务器之间的数据传输
- **双向数据传输**：可以在两个传输介质之间双向传输数据
- **数据拦截与过滤**：支持在数据传输过程中进行拦截和过滤
- **异步操作**：使用异步编程模型，提高性能和响应速度
- **自动重连机制**：当连接断开时，会自动尝试重连
- **构建器模式**：使用流畅的API创建传输线路
- **跨平台支持**：基于.NET平台，可以在Windows、Linux和macOS上运行

## 快速开始

### 基本用法

#### 1. 串口到串口的数据传输

```csharp
using DataShuttle;
using DataShuttle.Transports.SerialPort;

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
```

#### 2. TCP客户端到TCP客户端的数据传输

```csharp
using DataShuttle;
using DataShuttle.Transports.TcpClient;

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
```

## 项目结构

```
DataShuttle/
├── DataShuttle/                # 核心库
│   ├── ShuttleLine.cs         # 传输线路实现
│   └── ShuttleLineBuilder.cs  # 构建器实现
├── DataShuttle.Core/          # 核心接口和模型
│   ├── Helper/                # 辅助工具
│   ├── Interfaces/            # 接口定义
│   └── Models/                # 数据模型
├── DataShuttle.Transports/    # 传输实现
│   ├── SerialPort/            # 串口传输
│   ├── TcpClient/             # TCP客户端传输
│   └── TcpServer/             # TCP服务器传输
└── DataShuttle.Samples/       # 示例代码
    └── Program.cs             # 示例程序
```

## 核心组件

### ShuttleLine

`ShuttleLine` 是库的核心类，负责管理数据从一个传输源到另一个传输目标的流动。

- **双向传输**：同时处理两个方向的数据传输
- **数据拦截**：支持在数据传输过程中进行拦截和处理
- **状态管理**：跟踪传输线路的运行状态

### ITransport

`ITransport` 是所有传输实现的接口，定义了数据读写和连接管理的方法：

- **Write**：向传输介质写入数据
- **Read**：从传输介质读取数据
- **Run**：启动传输，建立连接
- **Stop**：停止传输，关闭连接
- **事件**：提供连接状态和错误状态的事件通知

### 传输实现

- **SerialPortTransport**：串口传输实现
- **TCPClientTransport**：TCP客户端传输实现
- **TcpServerTransport**：TCP服务器传输实现

## 高级用法

### 数据拦截与过滤

```csharp
var line = ShuttleLine.CreateBuilder()
    .AddFrom(SerialPortTransport.Create(new SerialPortTransportOptions()
    {
        PortName = "COM1"
    }))
    .AddTo(TCPClientTransport.Create(new TCPClientTransportOptions()
    {
        ServerIp = "127.0.0.1",
        ServerPort = 8888
    }))
    .AddFromDataIntercept(context =>
    {
        // 对从源传输来的数据进行处理
        Console.WriteLine($"接收到数据：{BitConverter.ToString(context.Data)}");
        
        // 可以修改数据
        // context.Data = new byte[] { 0x01, 0x02, 0x03 };
        
        // 可以取消传输
        // context.IsCancel = true;
    })
    .Build();
```

## 贡献指南

欢迎提交Issue和Pull Request来改进这个项目！

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/amazing-feature`)
3. 提交更改 (`git commit -m 'Add some amazing feature'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 打开Pull Request

## 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 联系方式

如有问题或建议，请通过GitHub Issues与我们联系。