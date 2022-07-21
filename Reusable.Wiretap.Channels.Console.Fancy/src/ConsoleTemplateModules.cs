using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Reusable.Essentials;
using Reusable.Fluorite;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Channels;

public static class ConsoleTemplateModules
{
    // Set style on the last element.
    public static Action<ILogEntry> Style<T>(this Action<ILogEntry> previous) where T : ConsoleStyle
    {
        return previous.Then(entry =>
        {
            if (Template(entry).Last() is IHtmlElement last)
            {
                last.attr("class", typeof(T).Name);
            }
        });
    }

    public static Action<ILogEntry> Whitespace(this Action<ILogEntry> previous, int length)
    {
        return previous.Then(entry => entry.Template().Text(new string(' ', length)));
    }

    public static Action<ILogEntry> Indent(this Action<ILogEntry> previous, int depth, int length = 3)
    {
        return previous.Whitespace(length * depth);
    }

    public static Action<ILogEntry> Text(this Action<ILogEntry> previous, object value)
    {
        return previous.Then(entry => entry.Template().Element("span", value));
    }

    public static Action<ILogEntry> Property<T>(this Action<ILogEntry> previous, string name, T fallback) where T : ILogProperty
    {
        return previous.Then(entry =>
        {
            var value = entry[name].Value switch { T v => v, _ => fallback };
            entry.Template().Element("span", value);
        });
    }

    public static Action<ILogEntry> Separator(this Action<ILogEntry> previous, string separator = " | ")
    {
        return previous.Then(entry => entry.Template().Element("span", separator));
    }

    public static Action<ILogEntry> Record(this Action<ILogEntry> previous, IEnumerable<string> values, IEnumerable<int> widths)
    {
        return previous.Then(entry => entry.Template().Elements("span", values.Pad(widths)));
    }

    public static IHtmlElement Template(this ILogEntry entry)
    {
        return entry[LogProperty.Names.Template()].Value as IHtmlElement;
    }
}