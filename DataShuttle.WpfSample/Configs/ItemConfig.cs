using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShuttle.WpfSample.Configs
{
    public partial class ItemConfig : ObservableObject
    {
        public TransportConfig FromTransportConfig { get; set; } = new TransportConfig();
        public TransportConfig ToTransportConfig { get; set; } = new TransportConfig();
        public string FromInterceptorScriptPath { get; set; }
        public string ToInterceptorScriptPath { get; set; }
    }
}
