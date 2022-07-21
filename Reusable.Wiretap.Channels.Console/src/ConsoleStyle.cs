using System;
using System.Collections;
using System.Collections.Generic;
using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Channels;

public interface IConsoleStyle
{
    ConsoleColor BackgroundColor { get; }

    ConsoleColor ForegroundColor { get; }
}

public interface IConditionalConsoleStyle : IConsoleStyle
{
    IConsoleStyle GetStyle(ILogEntry entry);
}

public record ConsoleStyle(ConsoleColor BackgroundColor, ConsoleColor ForegroundColor) : IConsoleStyle
{
    public static ConsoleStyle Current => new(Console.BackgroundColor, Console.ForegroundColor);

    public record LogLevel(ConsoleColor BackgroundColor, ConsoleColor ForegroundColor) : ConsoleStyle(BackgroundColor, ForegroundColor), IConditionalConsoleStyle, IEnumerable<IConsoleStyle>
    {
        public LogLevel(IConsoleStyle fallback) : this(fallback.BackgroundColor, fallback.ForegroundColor) { }

        private Dictionary<string, IConsoleStyle> Options { get; } = new(SoftString.Comparer);


        public IConsoleStyle GetStyle(ILogEntry entry)
        {
            return
                entry[LogProperty.Names.Level()].Value is Data.LogLevel level
                    ? Options.TryGetValue(level.ToString(), out var style)
                        ? style
                        : this
                    : this;
        }

        public IEnumerator<IConsoleStyle> GetEnumerator() => Options.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(IConsoleStyle style) => Options.Add(style.GetType().Name, style);

        #region Levels

        public record Trace() : ConsoleStyle(ConsoleColor.Black, ConsoleColor.DarkGray) { }

        public record Debug() : ConsoleStyle(ConsoleColor.Black, ConsoleColor.DarkGray) { }

        public record Information() : ConsoleStyle(ConsoleColor.Black, ConsoleColor.White) { }

        public record Warning() : ConsoleStyle(ConsoleColor.Black, ConsoleColor.Yellow) { }

        public record Error() : ConsoleStyle(ConsoleColor.Black, ConsoleColor.Red) { }

        public record Fatal() : ConsoleStyle(ConsoleColor.Black, ConsoleColor.DarkRed) { }

        #endregion
    }
}