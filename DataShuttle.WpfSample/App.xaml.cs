using DataShuttle.WpfSample.Configs;
using DataShuttle.WpfSample.ViewModels;
using System.Windows;

namespace DataShuttle.WpfSample
{
    public partial class App : Application
    {
        public static DefaultConfig<ConfigModel> Config { get; } = DefaultConfig<ConfigModel>.Instance;

        protected override async void OnExit(ExitEventArgs e)
        {
            if (MainWindow?.DataContext is MainWindowViewModel vm)
                await vm.StopAll();
            base.OnExit(e);
        }
    }
}
