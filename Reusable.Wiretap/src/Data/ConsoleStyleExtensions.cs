using System;
using Reusable.Marbles;

namespace Reusable.Wiretap.Data;

public static class ConsoleStyleExtensions
{
    public static IDisposable Apply(this IConsoleStyle style)
    {
        // Backup the current style.
        var (foregroundColor, backgroundColor) = ConsoleStyle.Current;

        // Apply a new stile.
        (Console.ForegroundColor, Console.BackgroundColor) = (style.ForegroundColor, style.BackgroundColor);

        // Restore the previous style on Dispose().
        return Disposable.Create(() =>
        {
            (Console.ForegroundColor, Console.BackgroundColor) = (foregroundColor, backgroundColor);
        });
    }
}