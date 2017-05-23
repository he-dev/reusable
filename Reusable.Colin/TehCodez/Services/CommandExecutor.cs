using System;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Colin.Collections;
using Reusable.Colin.Data;
using Reusable.Colin.Logging;

namespace Reusable.Colin.Services
{
    public class CommandExecutor
    {
        public CommandExecutor(ICommand command, ImmutableNameSet name, Type parameterType)
        {
            Command = command;

            Name =
                name.Any()
                    ? ImmutableNameSet.Create(name)
                    : ImmutableNameSet.From(command);

            CommandParameterFactory = new CommandParameterFactory(parameterType);
        }

        [PublicAPI]
        [NotNull]
        public ICommand Command { get; }

        [PublicAPI]
        [NotNull]
        public ImmutableNameSet Name { get; }

        [PublicAPI]
        [NotNull]
        public CommandParameterFactory CommandParameterFactory { get; }

        public void Execute(ArgumentLookup argument, CommandLine commandLine, ILogger logger)
        {
            var commandParameter = CommandParameterFactory.CreateParameter(argument);
            Command.Execute(new CommandContext(commandParameter, commandLine, logger));
        }
    }
}