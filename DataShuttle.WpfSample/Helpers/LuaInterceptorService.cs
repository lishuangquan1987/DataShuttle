using DataShuttle.Core.Models;
using NLua;
using System;
using System.Text;

namespace DataShuttle.WpfSample.Helpers
{
    /// <summary>
    /// 封装 NLua 调用，将 Lua 脚本编译为拦截器 Action
    /// 约定脚本中必须定义 function intercept(data) ... end
    /// 返回 byte[] 继续转发，返回 nil 则丢弃该包
    /// </summary>
    public class LuaInterceptorService : IDisposable
    {
        private Lua _lua;
        private LuaFunction _func;
        private string _lastError;

        public string LastError => _lastError;
        public bool HasError => _lastError != null;

        public bool Load(string script)
        {
            _lastError = null;
            try
            {
                _lua?.Dispose();
                _lua = new Lua();
                _lua.State.Encoding = Encoding.UTF8;
                _lua.DoString(script);
                _func = _lua["intercept"] as LuaFunction;
                if (_func == null)
                {
                    _lastError = "脚本中未找到 intercept 函数";
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                return false;
            }
        }

        public void Intercept(InterceptContext ctx)
        {
            if (_func == null) return;
            try
            {
                var results = _func.Call(ctx.Data);
                if (results == null || results.Length == 0 || results[0] == null)
                {
                    ctx.Cancel();
                    return;
                }

                // Lua 返回的可能是 byte[] 或 LuaTable
                if (results[0] is byte[] bytes)
                {
                    ctx.Data = bytes;
                }
                else if (results[0] is LuaTable table)
                {
                    ctx.Data = LuaTableToBytes(table);
                }
            }
            catch (Exception ex)
            {
                _lastError = ex.Message;
                // 出错不影响主流程，原样转发
            }
        }

        private static byte[] LuaTableToBytes(LuaTable table)
        {
            var list = new System.Collections.Generic.List<byte>();
            for (int i = 1; ; i++)
            {
                var val = table[i];
                if (val == null) break;
                list.Add(Convert.ToByte(val));
            }
            return list.ToArray();
        }

        public void Dispose()
        {
            _func = null;
            _lua?.Dispose();
            _lua = null;
        }
    }
}
