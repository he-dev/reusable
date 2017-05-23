using System;
using JetBrains.Annotations;
using Reusable.Colin.Logging;
using Reusable.Colin.Services;

namespace Reusable.Colin
{
    public class CommandLineSettings
    {
        [PublicAPI]
        public char ArgumentPrefix { get; set; }

        [PublicAPI]
        public char ArgumentValueSeparator { get; set; }

        [CanBeNull]
        [PublicAPI]
        public ILogger Logger { get; set; }

        [NotNull]
        public static CommandLineSettings Default => new CommandLineSettings
        {
            ArgumentPrefix = '-',
            ArgumentValueSeparator = ':',
            Logger = new ConsoleLogger()
        };
    }
}