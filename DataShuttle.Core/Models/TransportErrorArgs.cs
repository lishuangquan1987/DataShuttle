using System;

namespace DataShuttle.Core.Models
{
    public class TransportErrorArgs
    {
        /// <summary>发生错误的操作，如 "Write"、"Read"、"Connect"</summary>
        public string Operation { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }

        public static TransportErrorArgs Create(string operation, string message, Exception exception = null)
            => new TransportErrorArgs { Operation = operation, Message = message, Exception = exception };
    }
}
