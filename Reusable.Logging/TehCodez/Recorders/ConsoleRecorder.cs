using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.Logging.Loggex.Recorders
{
    public class ConsoleRecorder : IRecorder
    {
        public static readonly IReadOnlyDictionary<LogLevel, ConsoleColor> DefaultColors = new Dictionary<LogLevel, ConsoleColor>
        {
            [LogLevel.Trace] = ConsoleColor.DarkGray,
            [LogLevel.Debug] = ConsoleColor.DarkGray,
            [LogLevel.Info] = ConsoleColor.White,
            [LogLevel.Warn] = ConsoleColor.Yellow,
            [LogLevel.Error] = ConsoleColor.Red,
            [LogLevel.Fatal] = ConsoleColor.Red,
        };

        private readonly IReadOnlyDictionary<LogLevel, ConsoleColor> colors;

        public ConsoleRecorder([CanBeNull] IReadOnlyDictionary<LogLevel, ConsoleColor> colors = null)
        {
            colors = colors ?? DefaultColors;
        }

        public CaseInsensitiveString Name { get; set; } = "Console";

        public void Log(LogEntry logEntry)
        {
            if (colors.TryGetValue(logEntry.LogLevel(), out ConsoleColor color))
            {
                Console.ForegroundColor = color;
            }
            Console.WriteLine(logEntry.Message());
            Console.ResetColor();
        }
    }
}