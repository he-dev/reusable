using JetBrains.Annotations;

namespace Reusable.OmniLog
{
    public delegate bool LoggerPredicate(LogLevel logLevel, SoftString softString);
    
    public static class LoggerFilter
    {
        [NotNull]
        public static readonly LoggerPredicate Any = (logLevel, category) => true;
    }
}