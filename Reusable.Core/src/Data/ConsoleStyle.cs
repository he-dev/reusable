using System;

namespace Reusable.Data;

public record struct ConsoleStyle(ConsoleColor ForegroundColor, ConsoleColor BackgroundColor)
{
    public static ConsoleStyle Default { get; set; } = new(ConsoleColor.Black, ConsoleColor.Gray);
    
    public static ConsoleStyle Current => new(Console.BackgroundColor, Console.ForegroundColor);
}