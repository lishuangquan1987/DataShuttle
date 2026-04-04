using DataShuttle.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShuttle.Core.Models
{
    public class ShuttleLine : IShuttleLine
    {
        private ITransport _from;
        private ITransport _to;
        private Action<InterceptContext>? _fromDataIntercept;
        private Action<InterceptContext>? _toDataIntercept;
        public ShuttleLine() { }
        public ITransport From { get => _from; set => _from = value; }
        public ITransport To { get => _to; set => _to = value; }
        public Action<InterceptContext>? FromDataIntercept { get => _fromDataIntercept; set => _fromDataIntercept = value; }
        public Action<InterceptContext>? ToDataIntercept { get => _toDataIntercept; set => _toDataIntercept = value; }
    }
}
