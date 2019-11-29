using Reusable.Data;

namespace Reusable.OmniLog
{
    public abstract class LogLevel
    {
        public static Reusable.Data.Option<LogLevel> Trace { get; } = Reusable.Data.Option<LogLevel>.CreateWithCallerName();
        public static Reusable.Data.Option<LogLevel> Debug { get; } = Reusable.Data.Option<LogLevel>.CreateWithCallerName();
        public static Reusable.Data.Option<LogLevel> Information { get; } = Reusable.Data.Option<LogLevel>.CreateWithCallerName();
        public static Reusable.Data.Option<LogLevel> Warning { get; } = Reusable.Data.Option<LogLevel>.CreateWithCallerName();
        public static Reusable.Data.Option<LogLevel> Error { get; } = Reusable.Data.Option<LogLevel>.CreateWithCallerName();
        public static Reusable.Data.Option<LogLevel> Fatal { get; } = Reusable.Data.Option<LogLevel>.CreateWithCallerName();
    }
}