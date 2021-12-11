using System;

namespace Reusable.Wiretap.Connectors.Extensions;

public static class ConsoleStyleExtensions
{
    public static IDisposable Apply(this ConsoleStyle style)
    {
        // Backup the current style.
        var (foregroundColor, backgroundColor) = ConsoleStyle.Current;

        // Apply a new stile.
        (Console.ForegroundColor, Console.BackgroundColor) = (style.ForegroundColor, style.BackgroundColor);

        // Restore the previous style.
        return Disposable.Create(() =>
        {
            (Console.ForegroundColor, Console.BackgroundColor) = (foregroundColor, backgroundColor);
        });
    }
}