using System;
namespace DataShuttle.Core.Models
{
    public class OperationResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMsg { get; set; }

        public static OperationResult OK() => new OperationResult { IsSuccess = true };
        public static OperationResult NG(string errorMsg) => new OperationResult { IsSuccess = false, ErrorMsg = errorMsg };
        public static OperationResult NG(Exception e) => new OperationResult { IsSuccess = false, ErrorMsg = e.Message };
    }

    public class OperationResult<T> : OperationResult
    {
        public T Data { get; set; }

        public static OperationResult<T> OK(T data) => new OperationResult<T> { IsSuccess = true, Data = data };
        public static OperationResult<T> NG(string errorMsg) => new OperationResult<T> { IsSuccess = false, ErrorMsg = errorMsg };
        public static OperationResult<T> NG(Exception e) => new OperationResult<T> { IsSuccess = false, ErrorMsg = e.Message };
    }
}
