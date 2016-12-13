using System;
using System.Collections.Generic;

namespace Reusable.Shelly.Writers
{
    public class ConsoleLogger : Logger
    {
        private static readonly IReadOnlyDictionary<LogLevel, ConsoleColor> MessageLevelConsoleColor = new Dictionary<LogLevel, ConsoleColor>
        {
            { LogLevel.Trace, ConsoleColor.DarkGray },
            { LogLevel.Debug, ConsoleColor.DarkGray },
            { LogLevel.Info, ConsoleColor.White },
            { LogLevel.Warn, ConsoleColor.Yellow },
            { LogLevel.Error, ConsoleColor.Red },
        };

        protected override void Write(LogEntry logEntry)
        {
            Console.ForegroundColor = MessageLevelConsoleColor[logEntry.LogLevel];
            Console.Out.WriteLine(logEntry.Message);
            Console.ResetColor();
        }
    }
}
