using System;
using System.Threading;
using System.Threading.Tasks;
using DataShuttle.Core.Helper;
using DataShuttle.Core.Interfaces;
using DataShuttle.Core.Models;

namespace DataShuttle
{
    public class ShuttleLine : IShuttleLine
    {
        private ITransport _from;
        private ITransport _to;
        private Action<InterceptContext> _fromDataIntercept;
        private Action<InterceptContext> _toDataIntercept;
        private CancellationTokenSource _tokenSource;
        private bool _isRunning;

        public ShuttleLine()
        {
        }

        public ITransport From
        {
            get => _from;
            set => _from = value;
        }

        public ITransport To
        {
            get => _to;
            set => _to = value;
        }

        public Action<InterceptContext> FromDataIntercept
        {
            get => _fromDataIntercept;
            set => _fromDataIntercept = value;
        }

        public Action<InterceptContext> ToDataIntercept
        {
            get => _toDataIntercept;
            set => _toDataIntercept = value;
        }

        public bool IsRunning => _isRunning;

        public static ShuttleLineBuilder CreateBuilder() => new ShuttleLineBuilder();

        public void Dispose()
        {
            this.From?.Dispose();
            this.To?.Dispose();
        }

        public async Task Run()
        {
            if (_tokenSource != null) _tokenSource.Cancel();

            _tokenSource = new CancellationTokenSource();

            _ = Task.Factory.StartNew(async () =>
            {
                var token = _tokenSource.Token;

                _isRunning = true;
                await From.Run();
                await To.Run();

                var t1 = HandleData(From, To, FromDataIntercept, token);
                var t2 = HandleData(To, From, ToDataIntercept, token);

                await t1;
                await t2;

                await From.Stop();
                await To.Stop();
                _isRunning = false;
            });
        }

        public async Task Stop()
        {
            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
            }
        }

        private async Task HandleData(ITransport from, ITransport to, Action<InterceptContext> intercept,
            CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var readDataResult = await from.Read(token);
                if (!readDataResult.IsSuccess)
                {
                    if (!await MethodHelper.Delay(TimeSpan.FromMilliseconds(50), token))
                    {
                        return;
                    }

                    continue;
                }

                var context = new InterceptContext() { Data = readDataResult.Data };
                //过滤
                intercept?.Invoke(context);

                if (context.IsCancel) continue;

                var writeDataResult = await to.Write(context.Data, token);
                if (!writeDataResult.IsSuccess)
                {
                    if (!await MethodHelper.Delay(TimeSpan.FromMilliseconds(50), token))
                    {
                        return;
                    }

                    continue;
                }
            }
        }
    }
}