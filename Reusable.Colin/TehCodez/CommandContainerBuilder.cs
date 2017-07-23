using System;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.CommandLine.Collections;
using Reusable.CommandLine.Commands;
using Reusable.CommandLine.Data;
using Reusable.CommandLine.Services;

namespace Reusable.CommandLine
{
    public static class CommandContainerBuilder
    {
        [NotNull]
        public static CommandContainer Add<TCommand, TParameter>(this CommandContainer commandContainer)
            where TCommand : ICommand, new()
            where TParameter : new()
        {
            return commandContainer.Add(null, new TCommand(), typeof(TParameter));
        }

        [NotNull]
        public static CommandContainer Add<TCommand>(this CommandContainer commandContainer)
            where TCommand : ICommand, new()
        {
            return commandContainer.Add(null, new TCommand(), null);
        }

        [NotNull]
        public static CommandContainer Add<TParameter>(this CommandContainer commandContainer, ICommand command, params string[] names)
            where TParameter : new()
        {
            return commandContainer.Add(names.Any() ? ImmutableNameSet.Create(names) : null, command, typeof(TParameter));
        }

        [NotNull]
        public static CommandContainer Add(this CommandContainer commandContainer, ICommand command, params string[] names)
        {
            return commandContainer.Add(names.Any() ? ImmutableNameSet.Create(names) : null, command, null);
        }

        [NotNull]
        public static CommandContainer Add(this CommandContainer commandContainer, [NotNull] Action<object> action, [NotNull] params string[] names)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (names == null) throw new ArgumentNullException(nameof(names));
            if (!names.Any()) throw new ArgumentException(paramName: nameof(names), message: "You need to specify at least one name.");

            return commandContainer.Add(ImmutableNameSet.Create(names), new LambdaCommand(action), null);
        }

        [NotNull]
        private static CommandContainer Add(this CommandContainer commandContainer, IImmutableNameSet name, ICommand command, Type parameterType)
        {
            name = name ?? ImmutableNameSetFactory.CreateCommandNameSet(command);

            if (commandContainer.ContainsKey(name))
            {
                throw new ArgumentException($"A command with the name {name} already exists.");
            }

            if (!name.Any())
            {
                throw new ArgumentException("Command name must not be empty.");
            }

            return new CommandContainer(commandContainer.Add(name, new ConsoleCommand(command, name, parameterType)));
        }        
    }
}