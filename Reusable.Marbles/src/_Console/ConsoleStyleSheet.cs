using System;
using System.Collections;
using System.Collections.Generic;

namespace Reusable.Marbles;

public class ConsoleStyleSheet : IEnumerable<KeyValuePair<string, ConsoleStyle>>
{
    private IDictionary<string, ConsoleStyle> Styles { get; } = new Dictionary<string, ConsoleStyle>(SoftString.Comparer);

    public ConsoleStyleSheet(ConsoleStyle fallback) => Styles[string.Empty] = fallback;

    public ConsoleStyle this[string name]
    {
        get => Styles.TryGetValue(name, out var style) ? style : this[string.Empty];
        set => Styles[name] = value;
    }

    IEnumerator<KeyValuePair<string, ConsoleStyle>> IEnumerable<KeyValuePair<string, ConsoleStyle>>.GetEnumerator() => Styles.GetEnumerator();

    public IEnumerator GetEnumerator() => ((IEnumerable)Styles).GetEnumerator();

    //public void Add(string name, IConsoleStyle style) => Styles[name] = style;
}

public static class ConsoleStyleSheetExtensions
{
    public static ConsoleStyleSheet Add<T>(this ConsoleStyleSheet css) where T : ConsoleStyle, new()
    {
        return css.Also(x => x.Add(typeof(T).Name, new T()));
    }

    public static ConsoleStyleSheet Add(this ConsoleStyleSheet css, string name, ConsoleStyle style)
    {
        return css.Also(x => x[name] = style);
    }

    public static ConsoleStyleSheet Add<T>(this ConsoleStyleSheet css, T style) where T : ConsoleStyle
    {
        return css.Add(typeof(T).Name, style);
    }

    public static ConsoleStyleSheet Add(this ConsoleStyleSheet css, string name, ConsoleColor backgroundColor, ConsoleColor foregroundColor)
    {
        return css.Add(name, new ConsoleStyle(backgroundColor, foregroundColor));
    }
}