using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShuttle.Core.Models
{
    public class InterceptContext
    {
        internal bool _isNext;
        public void Next(byte[] data) 
        {
            _isNext = true;
            this.Data = data;
        }
        public byte[] Data { get; private set; }
    }
}
