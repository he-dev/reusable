using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Console;

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

        private readonly IConsoleRenderer _renderer;

        public ConsoleRx(IConsoleRenderer renderer)
        {
            _renderer = renderer;
        }

        public ConsoleRx() : this(new ConsoleRenderer()) { }

        protected override void Log(ILog log)
        {
            if (log.ConsoleTemplateBuilder() is var builder && !(builder is null))
            {
                _renderer.Render(builder.Build(log));
            }
            else
            {
                using (Disposable.Create(System.Console.ResetColor))
                {
                    if (Colorful && DefaultColors.TryGetValue(log.GetItemOrDefault<LogLevel>(LogPropertyNames.Level), out var consoleColor))
                    {
                        System.Console.ForegroundColor = consoleColor;
                    }

                    System.Console.WriteLine(log);
                }
            }
        }

        public bool Colorful { get; set; }

        public static IObserver<Log> Create(bool colorful = true)
        {
            return new ConsoleRx { Colorful = colorful };
        }
    }

    [UsedImplicitly]
    public class ColoredConsoleRx : LogRx
    {
        private readonly IConsoleRenderer _renderer;

        public ColoredConsoleRx(IConsoleRenderer renderer)
        {
            _renderer = renderer;
        }

        public ColoredConsoleRx()
            : this(new ConsoleRenderer()) { }

        public static ColoredConsoleRx Create(IConsoleRenderer renderer)
        {
            return new ColoredConsoleRx(renderer);
        }

        protected override void Log(ILog log)
        {
            if (log.ConsoleTemplateBuilder() is var builder && !(builder is null))
            {
                _renderer.Render(builder.Build(log));
            }
        }
    }
}