using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShuttle.WpfSample.Configs
{
    public class JsonHelper
    {
        private static readonly Encoding encoding = Encoding.UTF8;
        public static T DeSerialiseFromFile<T>(string path)
        {
            var text = File.ReadAllText(path, encoding);
            return JsonConvert.DeserializeObject<T>(text);
        }

        public static void SerializeToFile<T>(string path, T data)
        {
            var text = JsonConvert.SerializeObject(data);
            File.WriteAllText(path, text, encoding);
        }
    }
}
