using CommunityToolkit.Mvvm.ComponentModel;

namespace DataShuttle.WpfSample.Configs
{
    public class ItemConfig : ObservableObject
    {
        public TransportConfig FromTransportConfig { get; set; } = new TransportConfig();
        public TransportConfig ToTransportConfig { get; set; } = new TransportConfig();

        private string _fromInterceptorScript =
            "-- 左→右 拦截脚本\r\n-- 参数 data: byte[]\r\n-- 返回 byte[] 继续转发，返回 nil 丢弃\r\nfunction intercept(data)\r\n    return data\r\nend";

        public string FromInterceptorScript
        {
            get => _fromInterceptorScript;
            set => SetProperty(ref _fromInterceptorScript, value);
        }

        private string _toInterceptorScript =
            "-- 右→左 拦截脚本\r\n-- 参数 data: byte[]\r\n-- 返回 byte[] 继续转发，返回 nil 丢弃\r\nfunction intercept(data)\r\n    return data\r\nend";

        public string ToInterceptorScript
        {
            get => _toInterceptorScript;
            set => SetProperty(ref _toInterceptorScript, value);
        }
    }
}
