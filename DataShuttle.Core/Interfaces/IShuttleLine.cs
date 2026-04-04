using DataShuttle.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShuttle.Core.Interfaces
{
    public interface IShuttleLine
    {
        ITransport From { get; set; }
        ITransport To { get; set; }
        Action<InterceptContext>? FromDataIntercept { get; set; }
        Action<InterceptContext>? ToDataIntercept { get; set; }
    }
}
