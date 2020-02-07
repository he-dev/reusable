using System;
using System.Collections.Generic;
using Reusable.Data;
using Reusable.OmniLog.Abstractions.Data;
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

        public override void Log(LogEntry entry)
        {
            using (Disposable.Create(Console.ResetColor))
            {
                Console.ForegroundColor = GetConsoleColor(entry);
                base.Log(entry);
            }
        }

        private ConsoleColor GetConsoleColor(LogEntry entry)
        {
            if (!entry.TryGetProperty(LogEntry.Names.Level, m => m.ProcessWith<EchoNode>(), out var property))
            {
                return Console.ForegroundColor;
            }

            if (!LogLevelColor.TryGetValue(property.Value as Option<LogLevel> ?? LogLevel.Information, out var consoleColor))
            {
                return Console.ForegroundColor;
            }

            return consoleColor;
        }
    }
}