using System;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog
{
    public static class LogEntryExtensions
    {
        public static LogProperty? GetProperty(this ILogEntry entry, string name)
        {
            return entry.TryGetProperty(name, out var property) ? property : default;
        }

        public static ILogEntry Add(this ILogEntry entry, string name, object? value, Action<LogPropertyMeta.LogPropertyMetaBuilder> buildMeta)
        {
            entry.Add(new LogProperty(name, value, LogPropertyMeta.Builder.Pipe(buildMeta)));
            return entry;
        }
    }
}