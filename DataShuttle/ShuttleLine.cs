using DataShuttle.Core.Interfaces;
using DataShuttle.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShuttle
{
    public class ShuttleLine : IShuttleLine
    {
        private ITransport _from;
        private ITransport _to;
        private Action<InterceptContext>? _fromDataIntercept;
        private Action<InterceptContext>? _toDataIntercept;
        private CancellationTokenSource _tokenSource;
        private bool _isRunning;
        public ShuttleLine() { }
        public ITransport From { get => _from; set => _from = value; }
        public ITransport To { get => _to; set => _to = value; }
        public Action<InterceptContext>? FromDataIntercept { get => _fromDataIntercept; set => _fromDataIntercept = value; }
        public Action<InterceptContext>? ToDataIntercept { get => _toDataIntercept; set => _toDataIntercept = value; }

        public bool IsRunning => _isRunning;

        public ShuttleLineBuilder CreateBuilder() => new ShuttleLineBuilder();

        public void Dispose()
        {
            this.From?.Dispose();
            this.To?.Dispose();
        }

        public async Task Run()
        {
            _isRunning = true;

            if (_tokenSource != null) await _tokenSource.CancelAsync();

            _tokenSource = new CancellationTokenSource();

            await From.Start();
            await To.Start();

            var t1 = HandleData(From, To, FromDataIntercept, _tokenSource.Token);
            var t2 = HandleData(To, From, ToDataIntercept, _tokenSource.Token);

            await t1;
            await t2;

            await From.Stop();
            await To.Stop();
            _isRunning = false;
        }

        public async Task Stop()
        {
            if (_tokenSource != null)
            {
                await _tokenSource.CancelAsync();
            }
        }

        private async Task HandleData(ITransport from, ITransport to, Action<InterceptContext>? intercept, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var readDataResult = await from.Read(token);
                if (!readDataResult.IsSuccess) continue;

                var context = new InterceptContext() { Data = readDataResult.Data };
                //过滤
                intercept?.Invoke(context);

                if (!context.IsCancel) continue;

                var writeDataResult = await to.Write(context.Data, token);
                if (!writeDataResult.IsSuccess) continue;
            }

        }
    }
}
