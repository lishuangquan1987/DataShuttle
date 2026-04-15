using DataShuttle.WpfSample.ViewModels;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace DataShuttle.WpfSample
{
    public partial class MainWindow : HandyControl.Controls.Window
    {
        private ShuttleLineItemViewModel _subscribedItem;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();

            // 监听 SelectedItem 变化，切换日志滚动订阅
            if (DataContext is MainWindowViewModel vm)
            {
                vm.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName != nameof(MainWindowViewModel.SelectedItem)) return;
                    if (_subscribedItem != null)
                        _subscribedItem.Logs.CollectionChanged -= OnLogsChanged;
                    _subscribedItem = vm.SelectedItem;
                    if (_subscribedItem != null)
                        _subscribedItem.Logs.CollectionChanged += OnLogsChanged;
                };
            }
        }

        private void OnLogsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add) return;
            LogListBox.ScrollIntoView(LogListBox.Items[LogListBox.Items.Count - 1]);
        }
    }
}
