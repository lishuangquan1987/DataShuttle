using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataShuttle.WpfSample.Configs
{
    /// <summary>
    /// 对配置保存与加载的统一封装
    /// 继承类可以写成单例模式，在构造时加载配置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseConfig<T> where T : class, new()
    {
        private object lockObj = new object();
        private bool isSaving = false;
        public T Config { get; set; } = new T();
        public BaseConfig()
        {
            LoadConfig();
        }
        /// <summary>
        /// 需要子类重写配置保存在哪里
        /// </summary>
        public abstract string ConfigPath { get; }
        /// <summary>
        /// 子类在静态构造函数中构造单例
        /// </summary>

        protected virtual void LoadConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                return;
            }
            try
            {
                Config = JsonHelper.DeSerialiseFromFile<T>(ConfigPath) ?? new T();//序列化为空后，使用默认
            }
            catch (Exception e)
            {
                //反序列化出错，使用默认配置
                Config = new T();
            }
        }
        /// <summary>
        /// 保存配置
        /// </summary>
        /// <param name="isSaveImmediately">立即保存或者延迟保存，当并发保存时，延迟保存只会保存一次</param>
        public virtual void SaveConfig(bool isSaveImmediately = true)
        {
            var dir = Path.GetDirectoryName(ConfigPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (isSaveImmediately)
            {
                lock (lockObj)
                {
                    JsonHelper.SerializeToFile(ConfigPath, Config);
                }
            }
            else
            {
                if (isSaving) return;


                Task.Factory.StartNew(() =>
                {
                    isSaving = true;
                    Thread.Sleep(5000);
                    lock (lockObj)
                    {
                        JsonHelper.SerializeToFile(ConfigPath, Config);
                    }
                    isSaving = false;
                });
            }
        }
    }
}
