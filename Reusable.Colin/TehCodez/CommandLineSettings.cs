using System;
using System.Globalization;
using JetBrains.Annotations;
using Reusable.Loggex;
using Reusable.Loggex.Recorders;

namespace Reusable.CommandLine
{
    public class CommandLineSettings
    {
        [NotNull] private ILogger _logger;

        [NotNull] private CultureInfo _culture;

        [NotNull]
        public ILogger Logger
        {
            get => _logger;
            set => _logger = value ?? throw new ArgumentNullException(nameof(Logger));
        }

        [NotNull]
        public CultureInfo Culture
        {
            get => _culture;
            set => _culture = value ?? throw new ArgumentNullException(nameof(Culture));
        }

        [NotNull]
        public static CommandLineSettings Default => new CommandLineSettings
        {
            Logger = Loggex.Logger.Create("CommandLine", new LoggerConfiguration
            {
                Recorders = { new ConsoleRecorder() },
                Filters =
                {
                    new LogFilter
                    {
                        LogLevel = LogLevel.Debug,
                        Recorders = { "Console" }
                    }
                }
            }),
            Culture = CultureInfo.InvariantCulture
        };
    }
}