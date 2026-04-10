using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShuttle.WpfSample.Configs
{
    /// <summary>
    /// Config的默认实现，配置文件名称默认用T的名称
    /// 单例模式
    /// </summary>
    /// <typeparam name="T">配置的模型，配置文件为模型名称</typeparam>
    public sealed class DefaultConfig<T> : BaseConfig<T> where T : class, new()
    {
        public override string ConfigPath
        {
            get
            {

                if (typeof(T).IsGenericType)
                {
                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Config/{typeof(T).Namespace}.{typeof(T).Name}.{string.Join("_", typeof(T).GetGenericArguments().Select(x => x.FullName))}.config");
                }
                else
                {
                    return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Config/{typeof(T).FullName}.config");
                }
            }
        }
        public static DefaultConfig<T> Instance { get; }

        private DefaultConfig() { }

        static DefaultConfig()
        {
            Instance = new DefaultConfig<T>();
        }
    }
}
