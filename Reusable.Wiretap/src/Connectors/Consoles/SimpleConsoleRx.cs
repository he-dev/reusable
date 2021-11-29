using System;
using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Connectors
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
            if (entry.TryGetProperty(Names.Properties.Level, out var property) && property.Value is LogLevel logLevel && LogLevelColor.TryGetValue(logLevel, out var consoleColor))
            {
                return consoleColor;
            }

            return Console.ForegroundColor;
        }
    }
}