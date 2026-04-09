
namespace DataShuttle.Core.Models
{
    public class InterceptContext
    {
        public bool IsCancel { get; set; } = false;
        public void Cancel()
        {
            IsCancel = true;
        }
        public byte[] Data { get; set; }
    }
}
