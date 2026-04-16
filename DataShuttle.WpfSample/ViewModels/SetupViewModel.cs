using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataShuttle.Core.Models;
using DataShuttle.WpfSample.Configs;
using DataShuttle.WpfSample.Helpers;
using HandyControl.Tools.Extension;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows;

namespace DataShuttle.WpfSample.ViewModels
{
    public class SetupViewModel : ObservableObject, IDialogResultable<OperationResult<ItemConfig>>
    {
        public SetupViewModel()
        {
            ItemConfig = new ItemConfig();
            SureCmd = new AsyncRelayCommand(Sure);
            CancelCmd = new AsyncRelayCommand(Cancel);
            BindingPropertyChanged(ItemConfig);

            FromTransportConfigPropertyChanged(ItemConfig.FromTransportConfig, new PropertyChangedEventArgs(nameof(TransportConfig.Type)));
            ToTransportConfigPropertyChanged(ItemConfig.ToTransportConfig, new PropertyChangedEventArgs(nameof(TransportConfig.Type)));
        }

        public SetupViewModel(ItemConfig itemConfig) : this()
        {
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
            From串口ConfigVisibility = Visibility.Collapsed;
            FromTcpClientConfigVisibility = Visibility.Collapsed;
            FromTcpServerConfigVisibility = Visibility.Collapsed;
            FromUdpConfigVisibility = Visibility.Collapsed;
            FromWebSocketClientConfigVisibility = Visibility.Collapsed;
            FromWebSocketServerConfigVisibility = Visibility.Collapsed;
            var transportConfig = (TransportConfig)sender;
            switch (transportConfig.Type)
            {
                case "串口": From串口ConfigVisibility = Visibility.Visible; break;
                case "TCP客户端": FromTcpClientConfigVisibility = Visibility.Visible; break;
                case "TCP服务端": FromTcpServerConfigVisibility = Visibility.Visible; break;
                case "UDP": FromUdpConfigVisibility = Visibility.Visible; break;
                case "WebSocket客户端": FromWebSocketClientConfigVisibility = Visibility.Visible; break;
                case "WebSocket服务端": FromWebSocketServerConfigVisibility = Visibility.Visible; break;
            }
        }

        private void ToTransportConfigPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(TransportConfig.Type)) return;
            To串口ConfigVisibility = Visibility.Collapsed;
            ToTcpClientConfigVisibility = Visibility.Collapsed;
            ToTcpServerConfigVisibility = Visibility.Collapsed;
            ToUdpConfigVisibility = Visibility.Collapsed;
            ToWebSocketClientConfigVisibility = Visibility.Collapsed;
            ToWebSocketServerConfigVisibility = Visibility.Collapsed;
            var transportConfig = (TransportConfig)sender;
            switch (transportConfig.Type)
            {
                case "串口": To串口ConfigVisibility = Visibility.Visible; break;
                case "TCP客户端": ToTcpClientConfigVisibility = Visibility.Visible; break;
                case "TCP服务端": ToTcpServerConfigVisibility = Visibility.Visible; break;
                case "UDP": ToUdpConfigVisibility = Visibility.Visible; break;
                case "WebSocket客户端": ToWebSocketClientConfigVisibility = Visibility.Visible; break;
                case "WebSocket服务端": ToWebSocketServerConfigVisibility = Visibility.Visible; break;
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
        public List<int> BaudRates { get; } = new List<int> { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400, 460800, 921600 };

        public ItemConfig ItemConfig { get; private set; }
        public OperationResult<ItemConfig> Result { get; set; }
        public Action CloseAction { get; set; }
        public AsyncRelayCommand SureCmd { get; }
        public AsyncRelayCommand CancelCmd { get; }

        private Visibility _from串口ConfigVisibility;
        public Visibility From串口ConfigVisibility { get => _from串口ConfigVisibility; set => SetProperty(ref _from串口ConfigVisibility, value); }

        private Visibility _to串口ConfigVisibility;
        public Visibility To串口ConfigVisibility { get => _to串口ConfigVisibility; set => SetProperty(ref _to串口ConfigVisibility, value); }

        private Visibility _fromTcpClientConfigVisibility;
        public Visibility FromTcpClientConfigVisibility { get => _fromTcpClientConfigVisibility; set => SetProperty(ref _fromTcpClientConfigVisibility, value); }

        private Visibility _toTcpClientConfigVisibility;
        public Visibility ToTcpClientConfigVisibility { get => _toTcpClientConfigVisibility; set => SetProperty(ref _toTcpClientConfigVisibility, value); }

        private Visibility _fromTcpServerConfigVisibility;
        public Visibility FromTcpServerConfigVisibility { get => _fromTcpServerConfigVisibility; set => SetProperty(ref _fromTcpServerConfigVisibility, value); }

        private Visibility _toTcpServerConfigVisibility;
        public Visibility ToTcpServerConfigVisibility { get => _toTcpServerConfigVisibility; set => SetProperty(ref _toTcpServerConfigVisibility, value); }

        private Visibility _fromUdpConfigVisibility;
        public Visibility FromUdpConfigVisibility { get => _fromUdpConfigVisibility; set => SetProperty(ref _fromUdpConfigVisibility, value); }

        private Visibility _toUdpConfigVisibility;
        public Visibility ToUdpConfigVisibility { get => _toUdpConfigVisibility; set => SetProperty(ref _toUdpConfigVisibility, value); }

        private Visibility _fromWebSocketClientConfigVisibility;
        public Visibility FromWebSocketClientConfigVisibility { get => _fromWebSocketClientConfigVisibility; set => SetProperty(ref _fromWebSocketClientConfigVisibility, value); }

        private Visibility _toWebSocketClientConfigVisibility;
        public Visibility ToWebSocketClientConfigVisibility { get => _toWebSocketClientConfigVisibility; set => SetProperty(ref _toWebSocketClientConfigVisibility, value); }

        private Visibility _fromWebSocketServerConfigVisibility;
        public Visibility FromWebSocketServerConfigVisibility { get => _fromWebSocketServerConfigVisibility; set => SetProperty(ref _fromWebSocketServerConfigVisibility, value); }

        private Visibility _toWebSocketServerConfigVisibility;
        public Visibility ToWebSocketServerConfigVisibility { get => _toWebSocketServerConfigVisibility; set => SetProperty(ref _toWebSocketServerConfigVisibility, value); }
    }
}
