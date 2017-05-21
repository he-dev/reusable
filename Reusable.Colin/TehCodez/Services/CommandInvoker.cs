using System;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Colin.Collections;

namespace Reusable.Colin.Services
{
    public class CommandInvoker
    {
        public CommandInvoker(ICommand command, ImmutableNameSet name, Type parameterType)
        {
            Command = command;

            Name =
                name.Any()
                    ? ImmutableNameSet.Create(name)
                    : ImmutableNameSet.From(command);

            CommandParameterFactory = new CommandParameterFactory(parameterType);
        }

        [NotNull]
        public ICommand Command { get; }

        [NotNull]
        public ImmutableNameSet Name { get; }

        [NotNull]
        public CommandParameterFactory CommandParameterFactory { get; }

        public void Invoke(CommandLine commandLine, ArgumentLookup argument)
        {
            var commandParameter = CommandParameterFactory.CreateParameter(argument);
            Command.Execute(new ExecuteContext(commandLine, commandParameter));
        }
    }

    public class ExecuteContext
    {
        internal ExecuteContext(CommandLine commandLine, object parameter)
        {
            CommandLine = commandLine;
            Parameter = parameter;
        }

        [NotNull]
        public CommandLine CommandLine { get; }

        [CanBeNull]
        public object Parameter { get; }
    }
}