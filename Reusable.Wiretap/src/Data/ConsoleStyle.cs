using System;

namespace Reusable.Wiretap.Data;

public interface IConsoleStyle
{
    ConsoleColor BackgroundColor { get; }

    ConsoleColor ForegroundColor { get; }
}

public record ConsoleStyle(ConsoleColor BackgroundColor, ConsoleColor ForegroundColor) : IConsoleStyle
{
    public static ConsoleStyle Current => new(Console.BackgroundColor, Console.ForegroundColor);
}