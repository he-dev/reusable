using JetBrains.Annotations;

namespace Reusable.OmniLog
{
    public delegate bool LoggerPredicate([NotNull] LogLevel logLevel, [NotNull] SoftString category);
    
    public static class LoggerFilter
    {
        [NotNull]
        public static readonly LoggerPredicate Any = (logLevel, category) => true;
    }
}