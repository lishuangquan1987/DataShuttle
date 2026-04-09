using DataShuttle.Core.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DataShuttle.Core.Interfaces
{
    public interface ITransport:IDisposable
    {
        string Name { get; }
        Task<OperationResult> Write(byte[] data,CancellationToken token);

        /// <summary>
        /// Block when no data arrive
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<OperationResult<byte[]>> Read(CancellationToken token);

        event Action<bool> OnConnectionStatusChanged;

        bool IsConnected { get; }

        event Action<bool,string?> OnErrorStatusChanged;
        bool IsError { get; }
        string? ErrorMsg { get; }
        /// <summary>
        /// Start connect,when connect error,try all the time until Stop() called
        /// </summary>
        /// <returns></returns>
        Task Run();
        Task Stop();
    }
}
