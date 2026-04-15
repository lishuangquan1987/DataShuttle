using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataShuttle.Core.Models;
using DataShuttle.WpfSample.Configs;
using DataShuttle.WpfSample.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ShuttleLineClass = DataShuttle.ShuttleLine;

namespace DataShuttle.WpfSample.ViewModels
{
    public class ShuttleLineItemViewModel : ObservableObject
    {
        private ShuttleLineClass _shuttleLine;
        private LuaInterceptorService _fromInterceptor;
        private LuaInterceptorService _toInterceptor;

        public ItemConfig Config { get; }

        public ShuttleLineItemViewModel(ItemConfig config)
        {
            Config = config;
            StartCmd = new AsyncRelayCommand(Start, () => !IsRunning);
            StopCmd = new AsyncRelayCommand(Stop, () => IsRunning);
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            private set
            {
                if (SetProperty(ref _isRunning, value))
                {
                    OnPropertyChanged(nameof(ScriptEditorEnabled));
                    ((AsyncRelayCommand)StartCmd).NotifyCanExecuteChanged();
                    ((AsyncRelayCommand)StopCmd).NotifyCanExecuteChanged();
                }
            }
        }

        private bool _fromIsConnected;
        public bool FromIsConnected
        {
            get => _fromIsConnected;
            private set => SetProperty(ref _fromIsConnected, value);
        }

        private string _fromStatusText = "未连接";
        public string FromStatusText
        {
            get => _fromStatusText;
            private set => SetProperty(ref _fromStatusText, value);
        }

        private bool _toIsConnected;
        public bool ToIsConnected
        {
            get => _toIsConnected;
            private set => SetProperty(ref _toIsConnected, value);
        }

        private string _toStatusText = "未连接";
        public string ToStatusText
        {
            get => _toStatusText;
            private set => SetProperty(ref _toStatusText, value);
        }

        // 流量统计
        private long _fromBytes;
        private long _toBytes;

        private string _fromTrafficText = "";
        public string FromTrafficText
        {
            get => _fromTrafficText;
            private set => SetProperty(ref _fromTrafficText, value);
        }

        private string _toTrafficText = "";
        public string ToTrafficText
        {
            get => _toTrafficText;
            private set => SetProperty(ref _toTrafficText, value);
        }

        // 日志列表（替代 ErrorText）
        public ObservableCollection<string> Logs { get; } = new ObservableCollection<string>();

        private void AddLog(string msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var entry = $"[{DateTime.Now:HH:mm:ss}] {msg}";
                Logs.Add(entry);
                if (Logs.Count > 200) Logs.RemoveAt(0);
            });
        }

        public bool ScriptEditorEnabled => !IsRunning;

        public string FromSummary => Config.FromTransportConfig.Type + "  " + TransportFactory.GetSummary(Config.FromTransportConfig);
        public string ToSummary => Config.ToTransportConfig.Type + "  " + TransportFactory.GetSummary(Config.ToTransportConfig);

        public IAsyncRelayCommand StartCmd { get; }
        public IAsyncRelayCommand StopCmd { get; }

        private async Task Start()
        {
            var from = TransportFactory.Create(Config.FromTransportConfig);
            var to = TransportFactory.Create(Config.ToTransportConfig);

            if (from == null || to == null)
            {
                AddLog("Transport 配置不完整，请先编辑配置");
                return;
            }

            _fromBytes = 0;
            _toBytes = 0;
            FromTrafficText = "";
            ToTrafficText = "";

            from.OnConnectionStatusChanged += connected =>
                Application.Current.Dispatcher.Invoke(() =>
                {
                    FromIsConnected = connected;
                    FromStatusText = connected ? "已连接" : "未连接";
                });
            to.OnConnectionStatusChanged += connected =>
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ToIsConnected = connected;
                    ToStatusText = connected ? "已连接" : "未连接";
                });
            from.OnErrorStatusChanged += (isErr, msg) => { if (isErr) AddLog("[左] " + msg); };
            to.OnErrorStatusChanged += (isErr, msg) => { if (isErr) AddLog("[右] " + msg); };

            Action<InterceptContext> fromIntercept = null;
            Action<InterceptContext> toIntercept = null;

            if (!string.IsNullOrWhiteSpace(Config.FromInterceptorScript))
            {
                _fromInterceptor?.Dispose();
                _fromInterceptor = new LuaInterceptorService();
                if (_fromInterceptor.Load(Config.FromInterceptorScript))
                    fromIntercept = _fromInterceptor.Intercept;
                else
                    AddLog("[左脚本] " + _fromInterceptor.LastError);
            }

            if (!string.IsNullOrWhiteSpace(Config.ToInterceptorScript))
            {
                _toInterceptor?.Dispose();
                _toInterceptor = new LuaInterceptorService();
                if (_toInterceptor.Load(Config.ToInterceptorScript))
                    toIntercept = _toInterceptor.Intercept;
                else
                    AddLog("[右脚本] " + _toInterceptor.LastError);
            }

            // 在拦截器外层叠加流量计数
            Action<InterceptContext> fromWithStats = ctx =>
            {
                fromIntercept?.Invoke(ctx);
                if (!ctx.IsCancel)
                {
                    _fromBytes += ctx.Data?.Length ?? 0;
                    Application.Current.Dispatcher.Invoke(() =>
                        FromTrafficText = $"↑ {FormatBytes(_fromBytes)}");
                }
            };
            Action<InterceptContext> toWithStats = ctx =>
            {
                toIntercept?.Invoke(ctx);
                if (!ctx.IsCancel)
                {
                    _toBytes += ctx.Data?.Length ?? 0;
                    Application.Current.Dispatcher.Invoke(() =>
                        ToTrafficText = $"↑ {FormatBytes(_toBytes)}");
                }
            };

            _shuttleLine = ShuttleLineClass.CreateBuilder()
                .AddFrom(from)
                .AddTo(to)
                .AddFromIntercept(fromWithStats)
                .AddToIntercept(toWithStats)
                .Build();

            await _shuttleLine.Run();
            IsRunning = true;
            AddLog("已启动");
        }

        private async Task Stop()
        {
            if (_shuttleLine != null)
            {
                await _shuttleLine.Stop();
                _shuttleLine.Dispose();
                _shuttleLine = null;
            }

            _fromInterceptor?.Dispose();
            _fromInterceptor = null;
            _toInterceptor?.Dispose();
            _toInterceptor = null;

            IsRunning = false;
            FromIsConnected = false;
            FromStatusText = "未连接";
            ToIsConnected = false;
            ToStatusText = "未连接";
            AddLog("已停止");
        }

        public void RefreshSummaries()
        {
            OnPropertyChanged(nameof(FromSummary));
            OnPropertyChanged(nameof(ToSummary));
        }

        public async Task ForceStop() => await Stop();

        private static string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024):F1} MB";
        }
    }
}
