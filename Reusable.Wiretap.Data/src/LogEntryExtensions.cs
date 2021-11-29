using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Data;

public static class LogEntryExtensions
{
    public static ILogProperty? GetProperty(this ILogEntry entry, string name)
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
}