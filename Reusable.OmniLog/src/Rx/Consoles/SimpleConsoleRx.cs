using System;
using System.Collections.Generic;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog.Rx.Consoles
{
    public class SimpleConsoleRx : PlainConsoleRx
    {
        public IReadOnlyDictionary<Option<LogLevel>, ConsoleColor> LogLevelColor { get; set; } = new Dictionary<Option<LogLevel>, ConsoleColor>
        {
            [LogLevel.Trace] = ConsoleColor.DarkGray,
            [LogLevel.Debug] = ConsoleColor.DarkGray,
            [LogLevel.Information] = ConsoleColor.White,
            [LogLevel.Warning] = ConsoleColor.Yellow,
            [LogLevel.Error] = ConsoleColor.Red,
            [LogLevel.Fatal] = ConsoleColor.Red,
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
            if (entry[LogProperty.Names.Level]?.Value is Option<LogLevel> logLevel && LogLevelColor.TryGetValue(logLevel, out var consoleColor))
            {
                return consoleColor;
            }

            return Console.ForegroundColor;
        }
    }
}