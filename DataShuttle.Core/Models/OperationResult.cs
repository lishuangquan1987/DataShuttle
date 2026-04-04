using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShuttle.Core.Models
{
    public class OperationResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMsg { get; set; }
    }

    public class OperationResult<T>: OperationResult
    {
        public T? Data { get; set; }
    }
}
