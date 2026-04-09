using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataShuttle.Core.Helper
{
    public class MethodHelper
    {
        public static async Task<bool> Delay(TimeSpan ts, CancellationToken token = default)
        {
            try
            {
                await Task.Delay(ts, token);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<bool> Delay(CancellationToken token)
        {
            return await Delay(TimeSpan.FromMilliseconds(-1), token);
        }
    }
}
