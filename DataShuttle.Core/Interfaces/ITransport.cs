using DataShuttle.Core.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DataShuttle.Core.Interfaces
{
    public interface ITransport : IDisposable
    {
        string Name { get; }
        Task<OperationResult> Write(byte[] data, CancellationToken token);

        /// <summary>
        /// Block when no data arrive
        /// </summary>
        Task<OperationResult<byte[]>> Read(CancellationToken token);

        /// <summary>连接状态变化时触发，true=已连接，false=已断开</summary>
        event Action<bool> OnConnectionStatusChanged;
        bool IsConnected { get; }

        /// <summary>发生操作错误时触发（Write/Read/Connect），不维护持久状态</summary>
        event Action<TransportErrorArgs> OnError;

        /// <summary>
        /// Start connect, when connect error, try all the time until Stop() called
        /// </summary>
        Task Run();
        Task Stop();
    }
}
