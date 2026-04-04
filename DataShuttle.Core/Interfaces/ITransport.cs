using DataShuttle.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShuttle.Core.Interfaces
{
    public interface ITransport:IDisposable
    {
        Task<OperationResult> Write(byte[] data,CancellationToken token);

        /// <summary>
        /// Block when no data arrive
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<OperationResult<byte[]>> Read(CancellationToken token);

        bool IsConnected { get; }

        string? ErrorMsg { get; }
        /// <summary>
        /// Start connect,when connect error,try all the time until Stop() called
        /// </summary>
        /// <returns></returns>
        Task Start();
        Task Stop();
    }
}
