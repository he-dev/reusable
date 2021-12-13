using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Nodes;

/// <summary>
/// This node renames log properties.
/// </summary>
public class RenameProperty : LoggerNode
{
    public IList<IRename> Renames { get; set; } = new List<IRename>();

    public override void Invoke(ILogEntry entry)
    {
        foreach (var rename in Renames)
        {
            rename.Invoke(entry);
        }
        
        InvokeNext(entry);
    }
}

public interface IRename
{
    void Invoke(ILogEntry entry);
}

public class Remove : IRename
{
    public Regex? Pattern { get; set; }
    
    public void Invoke(ILogEntry entry)
    {
        var updates = LogEntry.Empty();
        foreach (var property in entry)
        {
            var newName = Pattern?.Replace(property.Name, string.Empty);
            if (newName?.Equals(property.Name) == false)
            {
                updates.Push(new LoggableProperty(newName, property.Name));
            }
        }

        entry.Merge(updates);
    }
}

public class Replace : IRename
{
    public Dictionary<string, string> Replacements { get; set; } = new();
    
    public void Invoke(ILogEntry entry)
    {
        foreach (var (key, newName) in Replacements.Select(x => (x.Key, x.Value)))
        {
            if (entry.TryGetProperty<LoggableProperty>(key, out var property))
            {
                entry.Push(new LoggableProperty(newName, property.Value));
            }
        }
    }
}