using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Fluorite;
using Reusable.Fluorite.Html;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Connectors;

[PublicAPI]
public class ConsoleConnectorFancy : ConsoleConnector<IHtmlElement>
{
    private object SyncLock { get; } = new();

    public ConsoleConnectorFancy() => Template = new HtmlMessageBuilder();

    /// <summary>
    /// Renders the Html to the console. This method is thread-safe.
    /// </summary>
    public override void Log(ILogEntry entry)
    {
        lock (SyncLock)
        {
            var rootElement =
                entry.TryGetProperty<ConsoleProperty.Template, IHtmlElement>(out var templateOverride)
                    ? templateOverride
                    : Template.Build(entry);

            using (Styles[rootElement.Name, entry].Apply())
            {
                Render(rootElement.AsEnumerable(), entry);
            }

            if (rootElement.Name.Equals("p"))
            {
                Console.WriteLine();
            }
        }
    }

    private void Render(IEnumerable<object> items, ILogEntry entry)
    {
        foreach (var item in items)
        {
            if (item is IHtmlElement element)
            {
                Render(element, entry);
            }
            else
            {
                Render(item);
            }
        }
    }

    private void Render(IHtmlElement element, ILogEntry entry)
    {
        using (Styles[element.Name, entry].Apply())
        {
            Render(element.AsEnumerable());
        }
    }

    private static void Render(object text)
    {
        Console.Write(text);
    }
}

public class HtmlMessageBuilder : IConsoleMessageBuilder<IHtmlElement>
{
    public Action<Action<ILogEntry>> Composition { get; set; } = previous =>
    {
        previous
            .Property<LoggableProperty.Timestamp>(DateTime.UtcNow)
            .Separator()
            .Property<LoggableProperty.Level>(LogLevel.Information).Style<ConsoleStyle.LogLevel>()
            .Separator()
            .Property<LoggableProperty.Logger>("Unknown")
            .Separator()
            .Property<LoggableProperty.Message>("None");
    };

    public IHtmlElement Build(ILogEntry entry)
    {
        Composition(ConsoleTemplate.ComposeLine());
        return entry.GetValueOrDefault<ConsoleProperty.Template, IHtmlElement>(default!);
    }
}

public abstract record ConsoleProperty(string Name, object Value) : MetaProperty(Name, Value)
{
    // Carries console template.
    public record Template(object Value) : ConsoleProperty(nameof(Template), Value);
}

public readonly record struct Quote(char Left, char Right)
{
    public static Quote Empty { get; } = new(default, default);

    public static Quote Single { get; } = new('\'', '\'');

    public static Quote Double { get; } = new('"', '"');

    public static Quote Square { get; } = new('[', ']');

    public static Quote Round { get; } = new('(', ')');

    public static Quote Curly { get; } = new('{', '}');

    public static Quote Angle { get; } = new('<', '>');

    public string Format(string value) => this == Empty ? value : $"{Left}{value}{Right}";
}