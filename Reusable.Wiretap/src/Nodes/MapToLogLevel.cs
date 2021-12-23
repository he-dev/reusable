using System.Collections.Generic;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Nodes;

/// <summary>
/// This nodes maps properties into log-levels.
/// </summary>
public class MapToLogLevel : LoggerNode
{
    public List<IMapToLogLevel> Mappers { get; set; } = new();

    public override void Invoke(ILogEntry entry)
    {
        foreach (var mapper in Mappers)
        {
            mapper.Invoke(entry);
        }

        InvokeNext(entry);
    }
}

public interface IMapToLogLevel
{
    void Invoke(ILogEntry entry);
}

public class MapPropertyToLogLevel<T> : IMapToLogLevel where T : notnull
{
    public MapPropertyToLogLevel(IEqualityComparer<T>? equalityComparer = default)
    {
        Mappings = new(equalityComparer ?? EqualityComparer<T>.Default);
        
    }
    public string PropertyName { get; set; } = default!;

    public Dictionary<T, LogLevel> Mappings { get; set; }

    public void Invoke(ILogEntry entry)
    {
        var level = LogLevel.Information;
        var canMap =
            entry.TryGetProperty(nameof(LoggableProperty.Level), out _) == false &&
            entry.TryGetProperty(PropertyName, out var property) &&
            property.Value is T value &&
            Mappings.TryGetValue(value, out level);

        if (canMap)
        {
            entry.Push(new LoggableProperty.Level(level));
        }
    }
}