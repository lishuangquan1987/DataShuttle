using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShuttle.WpfSample.Configs
{
    public class ConfigModel:ObservableObject
    {
        public List<ItemConfig> ItemConfigs { get; set; } = new List<ItemConfig>();
    }
}
