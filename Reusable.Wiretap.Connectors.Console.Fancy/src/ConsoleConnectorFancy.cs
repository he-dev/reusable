using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Htmlize.Html;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Connectors.Data;
using Reusable.Wiretap.Connectors.Extensions;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Connectors;

[PublicAPI]
public class ConsoleConnectorFancy : ConsoleConnector<IHtmlElement>
{
    private readonly object _syncLock = new();

    /// <summary>
    /// Renders the Html to the console. This method is thread-safe.
    /// </summary>
    public override void Log(ILogEntry entry)
    {
        lock (_syncLock)
        {
            var template =
                entry.TryGetProperty<RenderableProperty.Html, IBuilder<IHtmlElement>>(out var templateOverride)
                    ? templateOverride
                    : Template;


            var rootElement = template.Build(entry);

            using (rootElement.ParseConsoleStyleOrDefault(ConsoleStyle.Current).Apply())
            {
                Render(rootElement.AsEnumerable());
            }

            if (rootElement.Name.Equals("p"))
            {
                Console.WriteLine();
            }
        }
    }

    private static void Render(IEnumerable<object> values)
    {
        foreach (var value in values)
        {
            if (value is IHtmlElement htmlElement)
            {
                RenderSingle(htmlElement);
            }

            if (value is string text)
            {
                Render(text);
            }
        }
    }

    private static void RenderSingle(IHtmlElement htmlElement)
    {
        using (htmlElement.ParseConsoleStyleOrDefault(ConsoleStyle.Current).Apply())
        {
            Render(htmlElement.AsEnumerable());
        }
    }

    private static void Render(string text)
    {
        Console.Write(text);
    }
}

internal static class HtmlElementExtensions
{
    public static IHtmlElement SetStyle(this IHtmlElement element, ConsoleStyle? style)
    {
        return
            style.HasValue
                ? element
                    .backgroundColor(style.Value.BackgroundColor)
                    .color(style.Value.ForegroundColor)
                : element;
    }

    public static ConsoleStyle ParseConsoleStyleOrDefault(this IHtmlElement element, ConsoleStyle fallback)
    {
        if (!element.Attributes.TryGetValue("style", out var style))
        {
            return fallback;
        }

        var declarations = style.ToDeclarations().ToDictionary(x => x.property, x => x.value);

        declarations.TryGetValue("color", out var foregroundColor);
        declarations.TryGetValue("background-color", out var backgroundColor);

        return new ConsoleStyle
        (
            Enum.TryParse(backgroundColor, true, out ConsoleColor consoleBackgroundColor) ? consoleBackgroundColor : Console.BackgroundColor,
            Enum.TryParse(foregroundColor, true, out ConsoleColor consoleForegroundColor) ? consoleForegroundColor : Console.ForegroundColor
        );
    }
}

internal abstract record RenderableProperty(string Name, object Value) : ILogProperty
{
    public record Html(object Value) : RenderableProperty(nameof(Html), Value);
}

// Extensions

public static class Template
{
    public static Action<ILogEntry> Compose(ConsoleStyle? style = default)
    {
        return entry => entry.Push(new RenderableProperty.Html(HtmlElement.Builder.span().SetStyle(style)));
    }

    public static Action<ILogEntry> ComposeLine(ConsoleStyle? style = default)
    {
        return entry => entry.Push(new RenderableProperty.Html(HtmlElement.Builder.p().SetStyle(style)));
    }
}

public record Quote(char Left, char Right)
{
    public static Quote Empty { get; } = new(default, default);

    public record Single() : Quote('\'', '\'');

    public record Double() : Quote('"', '"');

    public record Square() : Quote('[', ']');

    public record Round() : Quote('(', ')');

    public record Curly() : Quote('{', '}');

    public record Angle() : Quote('<', '>');

    public string Format(string value) => this == Empty ? value : $"{Left}{value}{Right}";
}

public static class TemplateModules
{
    public static Action<ILogEntry> Whitespace(this Action<ILogEntry> previous, int length, int depth = 1, ConsoleStyle? style = default)
    {
        return previous.Then(entry => entry.Html().text(new string(' ', length * depth)).SetStyle(style));
    }

    public static Action<ILogEntry> Text(this Action<ILogEntry> previous, string text, Quote? quote = default, ConsoleStyle? style = default)
    {
        return previous.Then(entry => entry.Html().text((quote ?? Quote.Empty).Format(text)).SetStyle(style));
    }

    public static IHtmlElement Html(this ILogEntry entry) => (IHtmlElement)entry[nameof(RenderableProperty.Html)].Value;
}