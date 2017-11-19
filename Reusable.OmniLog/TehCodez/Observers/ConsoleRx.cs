using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using Reusable.OmniLog.Collections;

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
        
        protected override IObserver<Log> Initialize()
        {
            return Observer.Create<Log>(log =>
            {
                using (Disposable.Create(Console.ResetColor))
                {
                    if (Colorful && DefaultColors.TryGetValue(log.LogLevel(), out var consoleColor))
                    {
                        Console.ForegroundColor = consoleColor;
                    }
                    Console.WriteLine(log);
                }
            });
        }

        public bool Colorful { get; set; }

        public static IObserver<Log> Create(bool colorful = true)
        {
            return new ConsoleRx {Colorful = colorful};
        }
    }
}