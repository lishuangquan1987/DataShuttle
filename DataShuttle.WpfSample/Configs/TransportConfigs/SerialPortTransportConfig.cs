using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataShuttle.WpfSample.Configs.TransportConfigs
{
    public class SerialPortTransportConfig : ObservableObject
    {
        /*
         * public string PortName { get; set; } = "COM1";
        public int BauRate { get; set; } = 9600;
        public int Parity { get; set; } = (int)System.IO.Ports.Parity.None;
        public int DataBits { get; set; } = 8;
        public int StopBits { get; set; } = (int)System.IO.Ports.StopBits.One;

        public override string ToString()
        {
            return base.ToString();
        }
         */

        private string portName = "COM1";
        public string PortName
        {
            get => portName;
            set => SetProperty(ref portName, value);
        }

        private int bauRate = 9600;
        public int BauRate
        {
            get => bauRate;
            set => SetProperty(ref bauRate, value);
        }

        private int parity = (int)System.IO.Ports.Parity.None;
        public int Parity
        {
            get => parity;
            set => SetProperty(ref parity, value);
        }

        private int dataBits = 8;
        public int DataBits
        {
            get => dataBits;
            set => SetProperty(ref dataBits, value);
        }

        private int stopBits = (int)System.IO.Ports.StopBits.One;
        public int StopBits
        {
            get => stopBits;
            set => SetProperty(ref stopBits, value);
        }

    }
}
