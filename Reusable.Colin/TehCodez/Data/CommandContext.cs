using JetBrains.Annotations;
using Reusable.Colin.Logging;

namespace Reusable.Colin.Data
{
    public class CommandContext
    {
        internal CommandContext(object parameter, CommandLine commandLine, ILogger logger)
        {
            Parameter = parameter;
            CommandLine = commandLine;
            Logger = logger;
        }

        [CanBeNull]
        public object Parameter { get; }

        [NotNull]
        public CommandLine CommandLine { get; }

        [CanBeNull]
        public ILogger Logger { get; }
    }
}