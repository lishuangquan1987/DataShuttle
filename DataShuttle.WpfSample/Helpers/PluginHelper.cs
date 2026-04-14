using DataShuttle.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShuttle.WpfSample.Helpers
{
    public class PluginHelper
    {
        public static List<string> GetPluginNames()
        {
            var pluginType = typeof(DataShuttle.Core.Interfaces.ITransport);
            var plugins = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => pluginType.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract)
                .ToList();
            return plugins.Select(x => (Activator.CreateInstance(x) as ITransport).Name).ToList();
        }
    }
}
