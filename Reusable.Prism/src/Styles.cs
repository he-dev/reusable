using System;
using Reusable.Marbles;

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