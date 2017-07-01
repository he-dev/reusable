using JetBrains.Annotations;
using Reusable.Colin.Logging;
using Reusable.CommandLine.Logging;

namespace Reusable.CommandLine.Data
{
    public class CommandContext
    {
        internal CommandContext(object parameter, CommandCollection commandCollection, ILogger logger)
        {
            Parameter = parameter;
            CommandCollection = commandCollection;
            Logger = logger;
        }

        [CanBeNull]
        public object Parameter { get; }

        [NotNull]
        public CommandCollection CommandCollection { get; }

        [CanBeNull]
        public ILogger Logger { get; }
    }
}