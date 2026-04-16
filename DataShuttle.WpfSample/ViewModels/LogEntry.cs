using System;

namespace DataShuttle.WpfSample.ViewModels
{
    public enum LogLevel { Info, Warning, Error }

    public class LogEntry
    {
        public string Time { get; }
        public LogLevel Level { get; }
        public string Message { get; }

        // 颜色由 Level 决定，供 XAML DataTrigger 使用
        public string LevelTag => Level switch
        {
            LogLevel.Error => "ERR",
            LogLevel.Warning => "WRN",
            _ => "INF"
        };

        public LogEntry(LogLevel level, string message)
        {
            Time = DateTime.Now.ToString("HH:mm:ss");
            Level = level;
            Message = message;
        }
    }
}
