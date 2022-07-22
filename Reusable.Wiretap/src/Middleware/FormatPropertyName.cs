using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Nodes;

/// <summary>
/// This node formats the name of regular properties.
/// </summary>
public class FormatPropertyName : LoggerMiddleware
{
    //public override bool Enabled => MatchProperty is { } && MatchPattern is { };

    public Func<ILogProperty, bool> Matches { get; set; }

    public Func<string, string> Format { get; set; }

    public override void Invoke(ILogEntry entry)
    {
        var updates = LogEntry.Empty();
        foreach (var property in entry.WhereTag<IRegularProperty>().Where(Matches))
        {
            var newName = Format(property.Name);

            // Property name has been formatted.
            if (newName.Equals(property.Name) == false)
            {
                updates.Push(new LogProperty<IRegularProperty>(newName, property.Value));
            }

            // More than just the case changed. 
            if (newName.Equals(property.Name, StringComparison.OrdinalIgnoreCase) == false)
            {
                updates.Push(new LogProperty.Obsolete(property.Name));
            }
        }

        entry.Push(updates);

        Next?.Invoke(entry);
    }
}

public static class Trim
{
    public class Start : FormatPropertyName
    {
        public Start(string property, string value)
        {
            Matches = p => SoftString.Comparer.Equals(p.Name, property);
            Format = n => Regex.Replace(n, $"^{value}", string.Empty, RegexOptions.IgnoreCase);
        }
    }

    public class End : FormatPropertyName
    {
        public End(string property, string value)
        {
            Matches = p => SoftString.Comparer.Equals(p.Name, property);
            Format = n => Regex.Replace(n, $"{value}$", string.Empty, RegexOptions.IgnoreCase);
        }
    }
}

public class Capitalize : FormatPropertyName
{
    /// <summary>
    /// Capitalizes all properties.
    /// </summary>
    public Capitalize()
    {
        Matches = p => true;
        Format = n => Regex.Replace(n, @"\A([a-z])", m => m.Value.ToUpper());
    }

    /// <summary>
    /// Capitalizes only the specified property.
    /// </summary>
    public Capitalize(string property)
    {
        Matches = p => SoftString.Comparer.Equals(p.Name, property);
        Format = n => Regex.Replace(n, @"\A([a-z])", m => m.Value.ToUpper());
    }
}