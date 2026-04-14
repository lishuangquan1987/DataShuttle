using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DataShuttle.Core.Models;
using DataShuttle.WpfSample.Configs;
using DataShuttle.WpfSample.Helpers;
using System;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace DataShuttle.WpfSample.ViewModels
{
    public class ScriptEditorViewModel : ObservableObject
    {
        private static readonly string DefaultScript =
            "-- 参数 data: byte[]\r\n-- 返回 byte[] 继续转发，返回 nil 丢弃\r\nfunction intercept(data)\r\n    return data\r\nend";

        private ItemConfig _config;

        public ScriptEditorViewModel()
        {
            TestFromScriptCommand = new RelayCommand(TestFromScript);
            TestToScriptCommand = new RelayCommand(TestToScript);
            ClearFromScriptCommand = new RelayCommand(ClearFromScript);
            ClearToScriptCommand = new RelayCommand(ClearToScript);
        }

        public void SetConfig(ItemConfig config)
        {
            _config = config;
            OnPropertyChanged(nameof(FromScript));
            OnPropertyChanged(nameof(ToScript));
            FromTestResult = null;
            ToTestResult = null;
        }

        public string FromScript
        {
            get => _config?.FromInterceptorScript;
            set
            {
                if (_config != null) _config.FromInterceptorScript = value;
                OnPropertyChanged();
            }
        }

        public string ToScript
        {
            get => _config?.ToInterceptorScript;
            set
            {
                if (_config != null) _config.ToInterceptorScript = value;
                OnPropertyChanged();
            }
        }

        private string _fromTestInput = "01 02 03 04";
        public string FromTestInput
        {
            get => _fromTestInput;
            set => SetProperty(ref _fromTestInput, value);
        }

        private string _toTestInput = "01 02 03 04";
        public string ToTestInput
        {
            get => _toTestInput;
            set => SetProperty(ref _toTestInput, value);
        }

        private string _fromTestResult;
        public string FromTestResult
        {
            get => _fromTestResult;
            set => SetProperty(ref _fromTestResult, value);
        }

        private string _toTestResult;
        public string ToTestResult
        {
            get => _toTestResult;
            set => SetProperty(ref _toTestResult, value);
        }

        public ICommand TestFromScriptCommand { get; }
        public ICommand TestToScriptCommand { get; }
        public ICommand ClearFromScriptCommand { get; }
        public ICommand ClearToScriptCommand { get; }

        private void TestFromScript() => FromTestResult = RunTest(FromScript, FromTestInput);
        private void TestToScript() => ToTestResult = RunTest(ToScript, ToTestInput);

        private void ClearFromScript()
        {
            FromScript = DefaultScript;
            FromTestResult = null;
        }

        private void ClearToScript()
        {
            ToScript = DefaultScript;
            ToTestResult = null;
        }

        private static string RunTest(string script, string hexInput)
        {
            if (string.IsNullOrWhiteSpace(script))
                return "⚠ 脚本为空";

            byte[] inputBytes;
            try
            {
                inputBytes = hexInput
                    .Split(new char[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => Convert.ToByte(s, 16))
                    .ToArray();
            }
            catch
            {
                return "⚠ 输入格式错误，请输入十六进制字节，如：01 02 03";
            }

            using (var svc = new LuaInterceptorService())
            {
                if (!svc.Load(script))
                    return "✗ 脚本错误：" + svc.LastError;

                var ctx = new InterceptContext { Data = inputBytes };
                svc.Intercept(ctx);

                if (ctx.IsCancel)
                    return "→ 数据已丢弃（返回 nil）";

                var hex = BitConverter.ToString(ctx.Data).Replace("-", " ");
                var ascii = Encoding.ASCII.GetString(
                    Array.ConvertAll(ctx.Data, b => b >= 32 && b < 127 ? b : (byte)'.'));
                return "✓ 输出 [" + ctx.Data.Length + " 字节]\r\nHEX: " + hex + "\r\nASCII: " + ascii;
            }
        }
    }
}
