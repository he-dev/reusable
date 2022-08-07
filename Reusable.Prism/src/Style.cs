using System;
using System.Collections;
using System.Collections.Generic;
using Reusable.Marbles;
using Reusable.Prism.Elements;

namespace Reusable.Prism;

public record Style
{
    public Color? Color { get; init; }

    public Padding? Padding { get; init; }

    public Margin? Margin { get; init; }

    public int? Width { get; set; }
}

public record Color(ConsoleColor? Background = default, ConsoleColor? Foreground = default)
{
    public IDisposable Apply() => new Restore(Console.BackgroundColor, Console.ForegroundColor).Also(() =>
    {
        // Apply this style.
        if (Background.HasValue) Console.BackgroundColor = Background.Value;
        if (Foreground.HasValue) Console.ForegroundColor = Foreground.Value;
    });

    public static Color Current => new(Console.BackgroundColor, Console.ForegroundColor);

    private record Restore(ConsoleColor BackgroundColor, ConsoleColor ForegroundColor) : IDisposable
    {
        public void Dispose() => (Console.BackgroundColor, Console.ForegroundColor) = (BackgroundColor, ForegroundColor);
    }
}

public record Padding(int? Left = default, int? Right = default);

public record Margin(int? Left = default, int? Right = default);

public class ConsoleStyleSheet : IEnumerable<KeyValuePair<string, Style>>
{
    private IDictionary<string, Style> Styles { get; } = new Dictionary<string, Style>(SoftString.Comparer);

    public static ConsoleStyleSheet Empty() => new();

    public Style this[string name] => Styles.TryGetValue(name, out var style) ? style : throw DynamicException.Create($"Console style '{name}' not found.");

    IEnumerator<KeyValuePair<string, Style>> IEnumerable<KeyValuePair<string, Style>>.GetEnumerator() => Styles.GetEnumerator();

    public IEnumerator GetEnumerator() => ((IEnumerable)Styles).GetEnumerator();

    public void Add(string name, Style style) => Styles.Add(name, style);
}

public static class ConsoleStyleSheetExtensions
{
    public static ConsoleStyleSheet Add<T>(this ConsoleStyleSheet css) where T : Style, new()
    {
        return css.Also(x => x.Add(typeof(T).Name, new T()));
    }

    public static void Add(this ConsoleStyleSheet css, string name, Style left, Style text, Style right)
    {
        css.Add($"{name}:{nameof(Text.Quoted.Left)}", left);
        css.Add($"{name}", text);
        css.Add($"{name}:{nameof(Text.Quoted.Right)}", right);
    }
}