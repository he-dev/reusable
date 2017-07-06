using JetBrains.Annotations;
using Reusable.CommandLine.Logging;

namespace Reusable.CommandLine.Data
{
    public class CommandContext
    {
        internal CommandContext(object parameter, CommandContainer commandContainer, ILogger logger)
        {
            Parameter = parameter;
            CommandContainer = commandContainer;
            Logger = logger;
        }

        [CanBeNull]
        public object Parameter { get; }

        [NotNull]
        public CommandContainer CommandContainer { get; }

        [CanBeNull]
        public ILogger Logger { get; }
    }
}