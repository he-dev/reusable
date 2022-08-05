using System;
using System.Collections;
using System.Collections.Generic;
using Reusable.Marbles;

namespace Reusable.Marbles;

// public interface IConsoleStyle
// {
//     ConsoleColor BackgroundColor { get; }
//
//     ConsoleColor ForegroundColor { get; }
//
//     public IDisposable Apply() => new ConsoleStyle.Restore().Also(() =>
//     {
//         // Apply this style.
//         Console.BackgroundColor = BackgroundColor;
//         Console.ForegroundColor = ForegroundColor;
//     });
// }

// public interface ITerminal<T>
// {
//     TerminalStyle<T> Style { get; set; }
//
//     void WriteLine(string text);
//
//     void Write(string text);
// }
//
// public record TerminalStyle<T>(T BackgroundColor, T ForegroundColor)
// {
//     public Restore Apply(ITerminal<T> terminal) => new Restore(terminal.Style).Also(() => terminal.Style = new TerminalStyle<T>(BackgroundColor, ForegroundColor));
//
//     public static ConsoleStyle Current => new(Console.BackgroundColor, Console.ForegroundColor);
//
//     public record Restore(TerminalStyle<T> Style) : TerminalStyle<T>(Style.BackgroundColor, Style.ForegroundColor), IDisposable
//     {
//         public void Dispose() => (Console.BackgroundColor, Console.ForegroundColor) = (BackgroundColor, ForegroundColor);
//     }
// }

public record ConsoleStyle(ConsoleColor BackgroundColor, ConsoleColor ForegroundColor)
{
    public ConsoleStyle() : this(Console.BackgroundColor, Console.ForegroundColor) { }

    public Restore Apply() => new Restore().Also(() =>
    {
        // Apply this style.
        Console.BackgroundColor = BackgroundColor;
        Console.ForegroundColor = ForegroundColor;
    });

    public static ConsoleStyle Current => new(Console.BackgroundColor, Console.ForegroundColor);

    public record Restore : ConsoleStyle, IDisposable
    {
        public void Dispose() => (Console.BackgroundColor, Console.ForegroundColor) = (BackgroundColor, ForegroundColor);
    }
}