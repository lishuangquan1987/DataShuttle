using DataShuttle.WpfSample.ViewModels;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DataShuttle.WpfSample
{
    public partial class MainWindow : HandyControl.Controls.Window
    {
        private ShuttleLineItemViewModel _subscribedItem;
        private ScrollViewer _logScrollViewer;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();

            if (DataContext is MainWindowViewModel vm)
            {
                vm.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName != nameof(MainWindowViewModel.SelectedItem)) return;
                    if (_subscribedItem != null)
                        _subscribedItem.Logs.CollectionChanged -= OnLogsChanged;
                    _subscribedItem = vm.SelectedItem;
                    _logScrollViewer = null; // 切换 item 时重新获取
                    if (_subscribedItem != null)
                        _subscribedItem.Logs.CollectionChanged += OnLogsChanged;
                };
            }
        }

        private ScrollViewer GetLogScrollViewer()
        {
            if (_logScrollViewer != null) return _logScrollViewer;
            // ListBox 加载后才能找到内部 ScrollViewer
            if (VisualTreeHelper.GetChildrenCount(LogListBox) == 0) return null;
            var border = VisualTreeHelper.GetChild(LogListBox, 0);
            if (border is Decorator decorator)
                _logScrollViewer = decorator.Child as ScrollViewer;
            return _logScrollViewer;
        }

        private void OnLogsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add) return;

            // 用 BeginInvoke 延迟到集合变更通知完成后再滚动，避免在 CollectionChanged 回调中重入集合
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new System.Action(() =>
            {
                if (LogListBox.Items.Count == 0) return;
                var sv = GetLogScrollViewer();
                bool atBottom = sv == null || sv.VerticalOffset >= sv.ScrollableHeight - 2;
                if (atBottom)
                    LogListBox.ScrollIntoView(LogListBox.Items[LogListBox.Items.Count - 1]);
            }));
        }
    }
}
