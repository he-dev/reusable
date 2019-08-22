using System;
using System.Collections.Generic;
using Reusable.Data;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Rx.ConsoleRenderers
{
    public class SimpleConsoleRenderer : PlainConsoleRenderer
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

        public override void Render(LogEntry logEntry)
        {
            using (Disposable.Create(Console.ResetColor))
            {
                var logLevel = logEntry.GetItemOrDefault(LogEntry.Names.Level, default, LogLevel.Information);
                if (LogLevelColor.TryGetValue(logLevel, out var consoleColor))
                {
                    Console.ForegroundColor = consoleColor;
                }

                base.Render(logEntry);
            }
        }
    }
}