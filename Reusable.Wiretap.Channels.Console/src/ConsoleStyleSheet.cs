using System;
using System.Collections.Generic;
using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Channels;

public class ConsoleStyleSheet : Dictionary<string, IConsoleStyle>
{
    public ConsoleStyleSheet(IConsoleStyle fallback) : base(SoftString.Comparer) => this["Default"] = fallback;
    
    public IConsoleStyle this[string name, ILogEntry entry]
    {
        get
        {
            return
                TryGetValue(name, out var style)
                    ? style is IConditionalConsoleStyle conditional
                        ? conditional.GetStyle(entry)
                        : style
                    : this["Default"];
        }
    }
}

public static class ConsoleStyleSheetExtensions
{
    public static ConsoleStyleSheet Add<T>(this ConsoleStyleSheet css) where T : IConsoleStyle, new()
    {
        return css.Also(x => x.Add(typeof(T).Name, new T()));
    }

    public static ConsoleStyleSheet Add(this ConsoleStyleSheet css, string name, ConsoleColor backgroundColor, ConsoleColor foregroundColor)
    {
        return css.Also(x => x.Add(name, new ConsoleStyle(backgroundColor, foregroundColor)));
    }

    public static ConsoleStyleSheet Add<T>(this ConsoleStyleSheet css, T style) where T : IConsoleStyle
    {
        return css.Also(x => x.Add(typeof(T).Name, style));
    }
}