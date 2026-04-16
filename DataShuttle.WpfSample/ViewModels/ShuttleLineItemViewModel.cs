using CommunityToolkit.Mvvm.Input;
using DataShuttle.Core.Models;
using DataShuttle.WpfSample.Configs;
using DataShuttle.WpfSample.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
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
                    OnPropertyChanged(nameof(RunningStatusText));
                    OnPropertyChanged(nameof(RunningStatusColor));
                    ((AsyncRelayCommand)StartCmd).NotifyCanExecuteChanged();
                    ((AsyncRelayCommand)StopCmd).NotifyCanExecuteChanged();
                }
            }
        }

        /// <summary>运行状态文字，供列表卡片和状态面板使用</summary>
        public string RunningStatusText => IsRunning ? "运行中" : "已停止";
        /// <summary>运行状态颜色</summary>
        public string RunningStatusColor => IsRunning ? "#4CAF50" : "#9E9E9E";

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

        public ObservableCollection<LogEntry> Logs { get; } = new ObservableCollection<LogEntry>();

        private void AddLog(LogLevel level, string msg)
        {
            Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new System.Action(() =>
            {
                Logs.Add(new LogEntry(level, msg));
                if (Logs.Count > 500) Logs.RemoveAt(0);
            }));
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
                AddLog(LogLevel.Error, "Transport 配置不完整，请先编辑配置");
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
                    AddLog(LogLevel.Info, connected ? "[左] 已连接" : "[左] 连接断开");
                });
            to.OnConnectionStatusChanged += connected =>
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ToIsConnected = connected;
                    ToStatusText = connected ? "已连接" : "未连接";
                    AddLog(LogLevel.Info, connected ? "[右] 已连接" : "[右] 连接断开");
                });
            from.OnError += args => AddLog(LogLevel.Error, $"[左][{args.Operation}] {args.Message}");
            to.OnError += args => AddLog(LogLevel.Error, $"[右][{args.Operation}] {args.Message}");

            Action<InterceptContext> fromIntercept = null;
            Action<InterceptContext> toIntercept = null;

            if (!string.IsNullOrWhiteSpace(Config.FromInterceptorScript))
            {
                _fromInterceptor?.Dispose();
                _fromInterceptor = new LuaInterceptorService();
                if (_fromInterceptor.Load(Config.FromInterceptorScript))
                    fromIntercept = _fromInterceptor.Intercept;
                else
                    AddLog(LogLevel.Warning, "[左脚本] " + _fromInterceptor.LastError);
            }

            if (!string.IsNullOrWhiteSpace(Config.ToInterceptorScript))
            {
                _toInterceptor?.Dispose();
                _toInterceptor = new LuaInterceptorService();
                if (_toInterceptor.Load(Config.ToInterceptorScript))
                    toIntercept = _toInterceptor.Intercept;
                else
                    AddLog(LogLevel.Warning, "[右脚本] " + _toInterceptor.LastError);
            }

            Action<InterceptContext> fromWithStats = ctx =>
            {
                fromIntercept?.Invoke(ctx);
                if (!ctx.IsCancel)
                {
                    _fromBytes += ctx.Data?.Length ?? 0;
                    Application.Current.Dispatcher.Invoke(() =>
                        FromTrafficText = FormatBytes(_fromBytes));
                }
            };
            Action<InterceptContext> toWithStats = ctx =>
            {
                toIntercept?.Invoke(ctx);
                if (!ctx.IsCancel)
                {
                    _toBytes += ctx.Data?.Length ?? 0;
                    Application.Current.Dispatcher.Invoke(() =>
                        ToTrafficText = FormatBytes(_toBytes));
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
            AddLog(LogLevel.Info, "穿梭线已启动");
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
            AddLog(LogLevel.Info, "穿梭线已停止");
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
