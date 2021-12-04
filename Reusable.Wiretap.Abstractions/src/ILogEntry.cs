using System.Collections.Generic;

namespace Reusable.Wiretap.Abstractions;

public interface ILogEntry : IEnumerable<ILogProperty>
{
    ILogProperty this[string name] { get; }

    ILogEntry Push(ILogProperty property);

    bool TryGetProperty(string name, out ILogProperty property);
}

public static class LogEntryExtensions
{
    public static ILogProperty GetPropertyOrDefault(this ILogEntry entry, string name, ILogProperty defaultProperty)
    {
        return entry.TryGetProperty(name, out var property) ? property : defaultProperty;
    }

    public static T GetValueOrDefault<T>(this ILogEntry entry, string name, T defaultValue)
    {
        return
            entry.TryGetProperty(name, out var property) && property.Value is T result
                ? result
                : defaultValue;
    }
}