using System;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.CommandLine.Collections;
using Reusable.CommandLine.Commands;
using Reusable.CommandLine.Services;

namespace Reusable.CommandLine
{
    public static class CommandCollectionBuilder
    {
        [NotNull]
        public static CommandCollection Add<TCommand, TParameter>(this CommandCollection commandCollection)
            where TCommand : ICommand, new()
            where TParameter : new()
        {
            return commandCollection.Add(new CommandMapping(new TCommand(), ImmutableNameSet.Empty, typeof(TParameter)));
        }

        [NotNull]
        public static CommandCollection Add<TCommand>(this CommandCollection commandCollection)
            where TCommand : ICommand, new()
        {
            return commandCollection.Add(new CommandMapping(new TCommand(), ImmutableNameSet.Empty, null));
        }

        [NotNull]
        public static CommandCollection Add<TParameter>(this CommandCollection commandCollection, ICommand command, params string[] names)
            where TParameter : new()
        {
            return commandCollection.Add(new CommandMapping(command, ImmutableNameSet.Create(names), typeof(TParameter)));
        }

        [NotNull]
        public static CommandCollection Add(this CommandCollection commandCollection, ICommand command, params string[] names)
        {
            return commandCollection.Add(new CommandMapping(command, ImmutableNameSet.Create(names), null));
        }

        [NotNull]
        public static CommandCollection Add(this CommandCollection commandCollection, [NotNull] Action<object> action, [NotNull] params string[] names)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (names == null) throw new ArgumentNullException(nameof(names));
            if (!names.Any()) throw new ArgumentException(paramName: nameof(names), message: "You need to specify at least one name.");

            return commandCollection.Add(new CommandMapping(new SimpleCommand(action), ImmutableNameSet.Create(names), null));
        }

        [NotNull]
        private static CommandCollection Add(this CommandCollection commandCollection, CommandMapping commandMapping)
        {
            if (commandCollection.ContainsKey(commandMapping.Name))
            {
                throw new ArgumentException($"A command with the name {commandMapping.Name} already exists.");
            }

            return new CommandCollection(commandCollection.Add(commandMapping.Name, commandMapping));
        }

        [NotNull]
        [PublicAPI]
        public static CommandCollection Default(this CommandCollection commandCollection, string name)
        {
            if (commandCollection.ContainsKey(ImmutableNameSet.DefaultCommandName))
            {
                throw new ArgumentException("Default command is already added. There can be only one default command.");
            }

            if (commandCollection.Count < 2)
            {
                throw new ArgumentException("Default command can be added only when there are at least two commands.");
            }

            if (commandCollection.TryGetValue(ImmutableNameSet.Create(name), out CommandMapping invoker))
            {
                return new CommandCollection(commandCollection.Add(ImmutableNameSet.DefaultCommandName, invoker));
            }

            throw new ArgumentException($"Command {name} is not added yet.");
        }
    }
}