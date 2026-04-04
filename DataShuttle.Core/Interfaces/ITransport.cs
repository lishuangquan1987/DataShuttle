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
        Task<OperationResult> Write(byte[] data);

        Task<OperationResult<byte[]>> Read(byte[] data);
    }
}
