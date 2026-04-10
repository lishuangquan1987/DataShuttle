using DataShuttle.Core.Models;
using System;
using System.Threading.Tasks;

namespace DataShuttle.Core.Interfaces
{
    public interface IShuttleLine : IDisposable
    {
        bool IsRunning { get; }
        ITransport From { get; set; }
        ITransport To { get; set; }
        Action<InterceptContext> FromDataIntercept { get; set; }
        Action<InterceptContext> ToDataIntercept { get; set; }

        Task Run();
        Task Stop();
    }
}
