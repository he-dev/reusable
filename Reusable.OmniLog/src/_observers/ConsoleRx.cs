using System;
using System.Collections.Generic;
using Reusable.OmniLog.Abstractions;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog
{
    public class ConsoleRx : LogRx
    {
        public static readonly IReadOnlyDictionary<LogLevel, ConsoleColor> DefaultColors = new Dictionary<LogLevel, ConsoleColor>
        {
            [LogLevel.Trace] = ConsoleColor.DarkGray,
            [LogLevel.Debug] = ConsoleColor.DarkGray,
            [LogLevel.Information] = ConsoleColor.White,
            [LogLevel.Warning] = ConsoleColor.Yellow,
            [LogLevel.Error] = ConsoleColor.Red,
            [LogLevel.Fatal] = ConsoleColor.Red,
        };

        protected override void Log(ILog log)
        {
            using (Disposable.Create(Console.ResetColor))
            {
                if (Colorful && DefaultColors.TryGetValue(log.Level(), out var consoleColor))
                {
                    Console.ForegroundColor = consoleColor;
                }
                Console.WriteLine(log);
            }
        }

        public bool Colorful { get; set; }

        public static IObserver<Log> Create(bool colorful = true)
        {
            return new ConsoleRx { Colorful = colorful };
        }
    }
}