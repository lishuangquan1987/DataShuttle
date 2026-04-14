using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataShuttle.Core.Helper;
using DataShuttle.Core.Models;
using DataShuttle.WpfSample.Configs;
using DataShuttle.WpfSample.Helpers;
using HandyControl.Tools.Extension;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DataShuttle.WpfSample.ViewModels
{
    public class SetupViewModel : ObservableObject, IDialogResultable<OperationResult<ItemConfig>>
    {
        //新建时调用
        public SetupViewModel()
        {
            ItemConfig = new ItemConfig();
            SureCmd = new AsyncRelayCommand(Sure);
            CancelCmd = new AsyncRelayCommand(Cancel);
            BindingPropertyChanged(ItemConfig);

            FromTransportConfigPropertyChanged(ItemConfig.FromTransportConfig, new PropertyChangedEventArgs(nameof(TransportConfig.Type)) );
            ToTransportConfigPropertyChanged(ItemConfig.ToTransportConfig, new PropertyChangedEventArgs(nameof(TransportConfig.Type)));
        }

        public SetupViewModel(ItemConfig itemConfig) : this()
        {
            //复制一份
            var json = JsonConvert.SerializeObject(itemConfig);
            var copy = JsonConvert.DeserializeObject<ItemConfig>(json);

            UnBindingPropertyChanged(ItemConfig);
            ItemConfig = copy;
            BindingPropertyChanged(ItemConfig);

            FromTransportConfigPropertyChanged(ItemConfig.FromTransportConfig, new PropertyChangedEventArgs(nameof(TransportConfig.Type)));
            ToTransportConfigPropertyChanged(ItemConfig.ToTransportConfig, new PropertyChangedEventArgs(nameof(TransportConfig.Type)));
        }

        private void BindingPropertyChanged(ItemConfig itemConfig)
        {
            itemConfig.FromTransportConfig.PropertyChanged += FromTransportConfigPropertyChanged;
            itemConfig.ToTransportConfig.PropertyChanged += ToTransportConfigPropertyChanged;
        }
        private void UnBindingPropertyChanged(ItemConfig itemConfig)
        {
            itemConfig.FromTransportConfig.PropertyChanged -= FromTransportConfigPropertyChanged;
            itemConfig.ToTransportConfig.PropertyChanged -= ToTransportConfigPropertyChanged;
        }
        private void FromTransportConfigPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(TransportConfig.Type)) return;

            this.From串口ConfigVisibility = Visibility.Collapsed;
            this.FromTcpClientConfigVisibility = Visibility.Collapsed;
            this.FromTcpServerConfigVisibility = Visibility.Collapsed;

            var transportConfig = (TransportConfig)sender;
            switch (transportConfig.Type)
            {
                case "串口":
                    this.From串口ConfigVisibility = Visibility.Visible;
                    break;
                case "TCP客户端":
                    this.FromTcpClientConfigVisibility = Visibility.Visible;
                    break;
                case "TCP服务器":
                    this.FromTcpServerConfigVisibility = Visibility.Visible;
                    break;
            }
        }
        private void ToTransportConfigPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(TransportConfig.Type)) return;

            this.To串口ConfigVisibility = Visibility.Collapsed;
            this.ToTcpClientConfigVisibility = Visibility.Collapsed;
            this.ToTcpServerConfigVisibility = Visibility.Collapsed;

            var transportConfig = (TransportConfig)sender;
            switch (transportConfig.Type)
            {
                case "串口":
                    this.To串口ConfigVisibility = Visibility.Visible;
                    break;
                case "TCP客户端":
                    this.ToTcpClientConfigVisibility = Visibility.Visible;
                    break;
                case "TCP服务器":
                    this.ToTcpServerConfigVisibility = Visibility.Visible;
                    break;
            }
        }
        private async Task Cancel()
        {
            Result = OperationResult<ItemConfig>.NG("取消操作");
            CloseAction?.Invoke();
        }

        private async Task Sure()
        {
            Result = OperationResult<ItemConfig>.OK(ItemConfig);
            CloseAction?.Invoke();
        }

        public string[] SerialPorts => SerialPort.GetPortNames();
        public Parity[] Parities { get; } = Enum.GetValues(typeof(Parity)) as Parity[];
        public StopBits[] StopBits { get; } = Enum.GetValues(typeof(StopBits)) as StopBits[];
        public List<string> PluginNames { get; } = PluginHelper.GetPluginNames();

        public ItemConfig ItemConfig { get; private set; }

        public OperationResult<ItemConfig> Result { get; set; }
        public Action CloseAction { get; set; }

        public AsyncRelayCommand SureCmd { get; }
        public AsyncRelayCommand CancelCmd { get; }

        private Visibility _from串口ConfigVisibility;
        public Visibility From串口ConfigVisibility
        {
            get => _from串口ConfigVisibility;
            set => SetProperty(ref _from串口ConfigVisibility, value);
        }

        private Visibility _to串口ConfigVisibility;
        public Visibility To串口ConfigVisibility
        {
            get => _to串口ConfigVisibility;
            set => SetProperty(ref _to串口ConfigVisibility, value);
        }

        private Visibility _fromTcpClientConfigVisibility;
        public Visibility FromTcpClientConfigVisibility
        {
            get => _fromTcpClientConfigVisibility;
            set => SetProperty(ref _fromTcpClientConfigVisibility, value);
        }

        private Visibility _toTcpClientConfigVisibility;
        public Visibility ToTcpClientConfigVisibility
        {
            get => _toTcpClientConfigVisibility;
            set => SetProperty(ref _toTcpClientConfigVisibility, value);
        }

        private Visibility _fromTcpServerConfigVisibility;
        public Visibility FromTcpServerConfigVisibility
        {
            get => _fromTcpServerConfigVisibility;

            set => SetProperty(ref _fromTcpServerConfigVisibility, value);
        }

        private Visibility _toTcpServerConfigVisibility;
        public Visibility ToTcpServerConfigVisibility
        {
            get => _toTcpServerConfigVisibility;
            set => SetProperty(ref _toTcpServerConfigVisibility, value);
        }
    }
}
