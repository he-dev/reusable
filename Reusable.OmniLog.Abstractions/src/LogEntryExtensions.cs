using System;
using Reusable.Extensions;

namespace Reusable.OmniLog.Abstractions
{
    public static class LogEntryExtensions
    {
        public static LogProperty? GetProperty(this ILogEntry entry, SoftString name)
        {
            return entry.TryGetProperty(name, out var property) ? property : default;
        }

        public static ILogEntry Add(this ILogEntry entry, SoftString name, object? value, Action<LogPropertyMeta.LogPropertyMetaBuilder> buildMeta)
        {
            entry.Add(new LogProperty(name, value, LogPropertyMeta.Builder.Pipe(buildMeta)));
            return entry;
        }
    }
}