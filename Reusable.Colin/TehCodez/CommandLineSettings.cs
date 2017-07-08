using System;
using System.Globalization;
using JetBrains.Annotations;
using Reusable.CommandLine.Logging;
using Reusable.CommandLine.Logging.Loggers;

namespace Reusable.CommandLine
{
    public class CommandLineSettings
    {
        [NotNull] private ILogger _logger = new NullLogger();
        [NotNull] private CultureInfo _culture = CultureInfo.InvariantCulture;        

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
            Logger = new ConsoleLogger(),
            Culture = CultureInfo.InvariantCulture
        };
    }
}