using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataShuttle.Core.Models;
using DataShuttle.WpfSample.Configs;
using DataShuttle.WpfSample.Views;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DataShuttle.WpfSample.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public ObservableCollection<ShuttleLineItemViewModel> Items { get; } = new ObservableCollection<ShuttleLineItemViewModel>();
        public ScriptEditorViewModel ScriptEditor { get; } = new ScriptEditorViewModel();

        private ShuttleLineItemViewModel _selectedItem;
        public ShuttleLineItemViewModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    OnPropertyChanged(nameof(HasSelection));
                    ScriptEditor.SetConfig(value?.Config);
                }
            }
        }

        public bool HasSelection => SelectedItem != null;

        public MainWindowViewModel()
        {
            // 从持久化配置恢复
            foreach (var cfg in App.Config.Config.ItemConfigs)
                Items.Add(new ShuttleLineItemViewModel(cfg));
        }

        [RelayCommand]
        private async Task AddItem()
        {
            var result = await Dialog.Show(new SetupView())
                .GetResultAsync<OperationResult<ItemConfig>>();

            if (!result.IsSuccess) return;

            var cfg = result.Data;
            App.Config.Config.ItemConfigs.Add(cfg);
            App.Config.SaveConfig();

            var vm = new ShuttleLineItemViewModel(cfg);
            Items.Add(vm);
            SelectedItem = vm;
        }

        [RelayCommand]
        private async Task EditItem(ShuttleLineItemViewModel item)
        {
            if (item == null || item.IsRunning) return;

            var result = await Dialog.Show(new SetupView(item.Config))
                .GetResultAsync<OperationResult<ItemConfig>>();

            if (!result.IsSuccess) return;

            // SetupView 返回的是副本，需要把数据写回原 Config
            var updated = result.Data;
            item.Config.FromTransportConfig.Type = updated.FromTransportConfig.Type;
            item.Config.FromTransportConfig.SerialPortTransportConfig = updated.FromTransportConfig.SerialPortTransportConfig;
            item.Config.FromTransportConfig.TcpClientTransportConfig = updated.FromTransportConfig.TcpClientTransportConfig;
            item.Config.FromTransportConfig.TcpServerTransportConfig = updated.FromTransportConfig.TcpServerTransportConfig;
            item.Config.ToTransportConfig.Type = updated.ToTransportConfig.Type;
            item.Config.ToTransportConfig.SerialPortTransportConfig = updated.ToTransportConfig.SerialPortTransportConfig;
            item.Config.ToTransportConfig.TcpClientTransportConfig = updated.ToTransportConfig.TcpClientTransportConfig;
            item.Config.ToTransportConfig.TcpServerTransportConfig = updated.ToTransportConfig.TcpServerTransportConfig;

            App.Config.SaveConfig();
            item.RefreshSummaries();
        }

        [RelayCommand]
        private async Task DeleteItem(ShuttleLineItemViewModel item)
        {
            if (item == null) return;
            if (item.IsRunning) await item.ForceStop();

            App.Config.Config.ItemConfigs.Remove(item.Config);
            App.Config.SaveConfig();
            Items.Remove(item);

            if (SelectedItem == item) SelectedItem = null;
        }

        public async Task StopAll()
        {
            foreach (var item in Items)
                if (item.IsRunning)
                    await item.ForceStop();
        }
    }
}
