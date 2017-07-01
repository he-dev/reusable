using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Reusable.CommandLine.Logging;

namespace Reusable.Colin.Logging.Loggers
{
    [PublicAPI]
    public class ConsoleLogger : ILogger
    {
        public static readonly IImmutableDictionary<LogLevel, ConsoleColor> DefaultColors = new Dictionary<LogLevel, ConsoleColor>
            {
                [LogLevel.Debug] = ConsoleColor.DarkGray,
                [LogLevel.Warn] = ConsoleColor.Yellow,
                [LogLevel.Error] = ConsoleColor.Red,
            }
            .ToImmutableDictionary();

        private readonly IImmutableDictionary<LogLevel, ConsoleColor> _colors;

        public ConsoleLogger([CanBeNull] IDictionary<LogLevel, ConsoleColor> colors = null)
        {
            _colors = colors?.ToImmutableDictionary() ?? DefaultColors;
        }

        public ILogger Log(string message, LogLevel logLevel)
        {
            if (_colors.TryGetValue(logLevel, out ConsoleColor color))
            {
                Console.ForegroundColor = color;
            }
            Console.WriteLine(message);
            Console.ResetColor();
            return this;
        }
    }
}