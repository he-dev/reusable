using System.Globalization;
using JetBrains.Annotations;
using Reusable.CommandLine.Collections;
using Reusable.Logging.Loggex;

namespace Reusable.CommandLine.Data
{
    public class ConsoleContext
    {
        internal ConsoleContext(
            [NotNull] ArgumentLookup arguments, 
            [NotNull] CultureInfo culture,
            [NotNull] CommandContainer commands, 
            [NotNull] ILogger logger)
        {
            Arguments = arguments;
            Culture = culture;
            Commands = commands;
            Logger = logger;
        }

        [NotNull]
        public ArgumentLookup Arguments { get; }

        [NotNull]
        public CultureInfo Culture { get; }

        [NotNull]
        public CommandContainer Commands { get; }

        [NotNull]
        public ILogger Logger { get; }
    }
}