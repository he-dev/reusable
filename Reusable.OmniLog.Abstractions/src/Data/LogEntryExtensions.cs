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

        public static T GetValueOrDefault<T>(this ILogEntry entry, string name, T defaultValue)
        {
            return
                entry.TryGetProperty(name, out var property) && property.Value is T result
                    ? result
                    : defaultValue;
        }

        public static ILogEntry Add(this ILogEntry entry, string name, object value, Action<LogPropertyMeta.LogPropertyMetaBuilder> buildMeta)
        {
            return entry.Pipe(e => e.Push(new LogProperty(name, value, LogPropertyMeta.From(buildMeta))));
        }
    }
}