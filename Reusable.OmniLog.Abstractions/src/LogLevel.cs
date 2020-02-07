using Reusable.Data;

namespace Reusable.OmniLog.Abstractions
{
    public abstract class LogLevel
    {
        public static Option<LogLevel> Trace { get; } = Option<LogLevel>.CreateWithCallerName();
        public static Option<LogLevel> Debug { get; } = Option<LogLevel>.CreateWithCallerName();
        public static Option<LogLevel> Information { get; } = Option<LogLevel>.CreateWithCallerName();
        public static Option<LogLevel> Warning { get; } = Option<LogLevel>.CreateWithCallerName();
        public static Option<LogLevel> Error { get; } = Option<LogLevel>.CreateWithCallerName();
        public static Option<LogLevel> Fatal { get; } = Option<LogLevel>.CreateWithCallerName();
    }
}