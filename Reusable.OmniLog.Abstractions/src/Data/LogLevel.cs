using Reusable.Data;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog
{
    // public abstract class LogLevel
    // {
    //     public static Option<LogLevel> Trace { get; } = Option<LogLevel>.CreateWithCallerName();
    //     public static Option<LogLevel> Debug { get; } = Option<LogLevel>.CreateWithCallerName();
    //     public static Option<LogLevel> Information { get; } = Option<LogLevel>.CreateWithCallerName();
    //     public static Option<LogLevel> Warning { get; } = Option<LogLevel>.CreateWithCallerName();
    //     public static Option<LogLevel> Error { get; } = Option<LogLevel>.CreateWithCallerName();
    //     public static Option<LogLevel> Fatal { get; } = Option<LogLevel>.CreateWithCallerName();
    // }

    public enum LogLevel
    {
        None = 0,
        Trace,
        Debug,
        Information,
        Warning,
        Error,
        Fatal,
    }
}