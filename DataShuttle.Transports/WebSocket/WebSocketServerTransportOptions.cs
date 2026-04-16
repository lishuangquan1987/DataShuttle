namespace DataShuttle.Transports.WebSocket
{
    public class WebSocketServerTransportOptions
    {
        /// <summary>绑定端口</summary>
        public int BindingPort { get; set; }

        /// <summary>WebSocket 路由路径，例如 /ws</summary>
        public string Path { get; set; } = "/ws";
    }
}
