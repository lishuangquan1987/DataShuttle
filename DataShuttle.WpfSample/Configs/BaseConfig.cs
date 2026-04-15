using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DataShuttle.WpfSample.Configs
{
    public abstract class BaseConfig<T> where T : class, new()
    {
        private readonly object lockObj = new object();
        private volatile bool isSaving = false;

        public T Config { get; set; } = new T();

        public BaseConfig()
        {
            LoadConfig();
        }

        public abstract string ConfigPath { get; }

        protected virtual void LoadConfig()
        {
            if (!File.Exists(ConfigPath)) return;
            try
            {
                Config = JsonHelper.DeSerialiseFromFile<T>(ConfigPath) ?? new T();
            }
            catch
            {
                Config = new T();
            }
        }

        public virtual void SaveConfig(bool isSaveImmediately = true)
        {
            var dir = Path.GetDirectoryName(ConfigPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

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
