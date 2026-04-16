using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataShuttle.Core.Models;
using DataShuttle.WpfSample.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace DataShuttle.WpfSample.ViewModels
{
    public partial class LuaEditorViewModel : ObservableObject
    {
        private static readonly string DefaultScript =
            "-- 参数 data: byte[]\r\n-- 返回 byte[] 继续转发，返回 nil 丢弃\r\nfunction intercept(data)\r\n    return data\r\nend";

        // AvalonEdit 不支持 XAML 双向绑定，由 TextChangedCommand 同步
        private string _script;
        public string Script
        {
            get => _script;
            set => SetProperty(ref _script, value);
        }

        [ObservableProperty] private string _testInput = "01 02 03 04";
        [ObservableProperty] private string _testOutput;
        [ObservableProperty] private string _statusText;

        /// <summary>保存时由 Window 订阅，关闭并回传脚本</summary>
        public event Action<string> SaveRequested;
        public event Action CancelRequested;

        // ── 由 View 桥接调用（AvalonEdit 不支持 XAML Behavior）────

        /// <summary>View 直接调用，同步编辑器文本到 Script</summary>
        public void SyncScript(string text) => Script = text;

        /// <summary>光标位置变化 → 更新状态栏</summary>
        public void UpdateCaretStatus(int line, int column) =>
            StatusText = $"行 {line}  列 {column}";

        /// <summary>KeyDown → F5 触发测试</summary>
        [RelayCommand]
        private void KeyDown(KeyEventArgs e)
        {
            if (e?.Key == Key.F5)
            {
                e.Handled = true;
                Test();
            }
        }

        // ── 业务命令 ─────────────────────────────────────────────

        [RelayCommand]
        private void Test()
        {
            byte[] inputBytes;
            try
            {
                inputBytes = TestInput
                    .Split(new char[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => Convert.ToByte(s, 16))
                    .ToArray();
            }
            catch
            {
                TestOutput = "⚠ 输入格式错误，请输入十六进制字节，如：01 02 03";
                return;
            }

            using var svc = new LuaInterceptorService();
            var logs = new List<string>();
            svc.OnLog = msg => logs.Add(msg);

            if (!svc.Load(Script))
            {
                TestOutput = "✗ 脚本错误：\r\n" + svc.LastError;
                return;
            }

            var ctx = new InterceptContext { Data = inputBytes };
            svc.Intercept(ctx);

            var sb = new StringBuilder();
            if (logs.Count > 0)
                sb.AppendLine("📋 日志:\r\n" + string.Join("\r\n", logs) + "\r\n");

            if (ctx.IsCancel)
                sb.Append("→ 数据已丢弃（返回 nil）");
            else
            {
                var hex = BitConverter.ToString(ctx.Data).Replace("-", " ");
                var ascii = Encoding.ASCII.GetString(
                    Array.ConvertAll(ctx.Data, b => b >= 32 && b < 127 ? b : (byte)'.'));
                sb.Append($"✓ 输出 [{ctx.Data.Length} 字节]\r\nHEX:   {hex}\r\nASCII: {ascii}");
            }
            TestOutput = sb.ToString();
        }

        [RelayCommand]
        private void Reset()
        {
            Script = DefaultScript;
        }

        [RelayCommand]
        private void Save() => SaveRequested?.Invoke(Script);

        [RelayCommand]
        private void Cancel() => CancelRequested?.Invoke();
    }
}
