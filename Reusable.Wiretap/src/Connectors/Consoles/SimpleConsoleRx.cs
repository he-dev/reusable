using System;
using System.Collections.Generic;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Connectors
{
    using static LogLevel;

    public class SimpleConsoleRx : PlainConsoleRx
    {
        public IReadOnlyDictionary<LogLevel, ConsoleColor> LogLevelColor { get; set; } = new Dictionary<LogLevel, ConsoleColor>
        {
            [Trace] = ConsoleColor.DarkGray,
            [Debug] = ConsoleColor.DarkGray,
            [Information] = ConsoleColor.White,
            [Warning] = ConsoleColor.Yellow,
            [Error] = ConsoleColor.Red,
            [Fatal] = ConsoleColor.Red,
        };

        public override void Log(ILogEntry entry)
        {
            using (Disposable.Create(Console.ResetColor))
            {
                Console.ForegroundColor = GetConsoleColor(entry);
                base.Log(entry);
            }
        }

        private ConsoleColor GetConsoleColor(ILogEntry entry)
        {
            if (entry.TryGetProperty(nameof(LoggableProperty.Level), out var property) && property?.Value is LogLevel logLevel && LogLevelColor.TryGetValue(logLevel, out var consoleColor))
            {
                return consoleColor;
            }

            return Console.ForegroundColor;
        }
    }
}