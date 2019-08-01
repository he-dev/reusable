using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Console;

// ReSharper disable once CheckNamespace
namespace Reusable.OmniLog
{
    [PublicAPI]
    public class ConsoleRx : LogRx
    {
//        public static readonly IReadOnlyDictionary<LogLevel, ConsoleColor> DefaultColors = new Dictionary<LogLevel, ConsoleColor>
//        {
//            [LogLevel.Trace] = ConsoleColor.DarkGray,
//            [LogLevel.Debug] = ConsoleColor.DarkGray,
//            [LogLevel.Information] = ConsoleColor.White,
//            [LogLevel.Warning] = ConsoleColor.Yellow,
//            [LogLevel.Error] = ConsoleColor.Red,
//            [LogLevel.Fatal] = ConsoleColor.Red,
//        };

        public static readonly ConsoleStyle DefaultStyle = new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Gray);

        private readonly IConsoleRenderer _renderer;

        public ConsoleRx(IConsoleRenderer renderer)
        {
            _renderer = renderer;
        }

        public ConsoleRx() : this(new ConsoleRenderer()) { }

        public ConsoleStyle Style { get; set; } = DefaultStyle;

        [CanBeNull]
        public ConsoleTemplateBuilder TemplateBuilder { get; set; }

        protected override void Log(ILog log)
        {
            if ((log.ConsoleTemplateBuilder() ?? TemplateBuilder) is var templateBuilder && templateBuilder is null)
            {
                return;
            }

            if (!log.ConsoleStyle().HasValue)
            {
                log.ConsoleStyle(Style);
            }

            _renderer.Render(templateBuilder.Build(log));
            
//            else
//            {
//                using (Disposable.Create(System.Console.ResetColor))
//                {
//                    if (Colorful && DefaultColors.TryGetValue(log.GetItemOrDefault<LogLevel>(LogPropertyNames.Level), out var consoleColor))
//                    {
//                        System.Console.ForegroundColor = consoleColor;
//                    }
//
//                    System.Console.WriteLine(log);
//                }
//            }
        }

        //public bool Colorful { get; set; }

//        public static IObserver<Log> Create(bool colorful = true)
//        {
//            return new ConsoleRx { Colorful = colorful };
//        }
    }
}