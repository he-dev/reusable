using System;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Middleware;

/// <summary>
/// This node formats the name of regular properties.
/// </summary>
public abstract class FormatPropertyName : LoggerMiddleware
{
    protected Func<string, bool> Matches { get; set; } = default!;

    protected Func<string, string> Format { get; set; } = default!;

    public override void Invoke(ILogEntry entry)
    {
        var updates = LogEntry.Empty();
        foreach (var property in entry.OfType<IRegularProperty>().Where(p => Matches(p.Name)))
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
            Matches = n => SoftString.Comparer.Equals(n, property);
            Format = n => Regex.Replace(n, $"^{value}", string.Empty, RegexOptions.IgnoreCase);
        }
    }

    public class End : FormatPropertyName
    {
        public End(string property, string value)
        {
            Matches = n => SoftString.Comparer.Equals(n, property);
            Format = n => Regex.Replace(n, $"{value}$", string.Empty, RegexOptions.IgnoreCase);
        }
    }
}

public class Capitalize : FormatPropertyName
{
    /// <summary>
    /// Capitalizes all properties by default or the specified one.
    /// </summary>
    public Capitalize(string? property = default)
    {
        Matches = n => property is null || SoftString.Comparer.Equals(n, property);
        Format = n => Regex.Replace(n, @"\A([a-z])", m => m.Value.ToUpper());
    }
}