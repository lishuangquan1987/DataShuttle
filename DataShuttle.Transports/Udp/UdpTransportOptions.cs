namespace DataShuttle.Transports.Udp
{
    public class UdpTransportOptions
    {
        /// <summary>本地绑定端口，用于接收数据</summary>
        public int BindingPort { get; set; }

        /// <summary>发送目标 IP</summary>
        public string RemoteIp { get; set; }

        /// <summary>发送目标端口</summary>
        public int RemotePort { get; set; }
    }
}
