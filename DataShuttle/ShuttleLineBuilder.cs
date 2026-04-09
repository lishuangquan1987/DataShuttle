using System;
using DataShuttle.Core.Interfaces;
using DataShuttle.Core.Models;

namespace DataShuttle
{
    public class ShuttleLineBuilder
    {
        private ShuttleLine _shuttleLine = new ShuttleLine();
        public ShuttleLineBuilder AddFrom(ITransport from)
        {
            this._shuttleLine.From = from;
            return this;
        }
        public ShuttleLineBuilder AddTo(ITransport to)
        {
            this._shuttleLine.To = to;
            return this;
        }
        public ShuttleLineBuilder AddFromIntercept(Action<InterceptContext> fromIntercept)
        {
            this._shuttleLine.FromDataIntercept = fromIntercept;
            return this;
        }

        public ShuttleLineBuilder AddToIntercept(Action<InterceptContext> toIntercept)
        {
            this._shuttleLine.ToDataIntercept = toIntercept;
            return this;
        }

        public ShuttleLine Build()
        {
            return this._shuttleLine;
        }
    }
}
