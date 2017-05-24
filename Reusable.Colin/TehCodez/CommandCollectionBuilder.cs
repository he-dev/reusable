using System;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Colin.Collections;
using Reusable.Colin.Commands;
using Reusable.Colin.Services;

namespace Reusable.Colin
{
    public static class CommandCollectionBuilder
    {
        [NotNull]
        [PublicAPI]
        public static CommandCollection Add<TCommand, TParameter>(this CommandCollection commandCollection)
            where TCommand : ICommand, new()
            where TParameter : new()
        {
            return commandCollection.Add(new Services.CommandExecutor(new TCommand(), ImmutableNameSet.Empty, typeof(TParameter)));
        }

        [NotNull]
        [PublicAPI]
        public static CommandCollection Add<TCommand>(this CommandCollection commandCollection)
            where TCommand : ICommand, new()
        {
            return commandCollection.Add(new Services.CommandExecutor(new TCommand(), ImmutableNameSet.Empty, null));
        }

        [NotNull]
        [PublicAPI]
        public static CommandCollection Add<TParameter>(this CommandCollection commandCollection, ICommand command, params string[] names)
            where TParameter : new()
        {
            return commandCollection.Add(new Services.CommandExecutor(command, ImmutableNameSet.Create(names), typeof(TParameter)));
        }

        [NotNull]
        [PublicAPI]
        public static CommandCollection Add(this CommandCollection commandCollection, ICommand command, params string[] names)
        {
            return commandCollection.Add(new Services.CommandExecutor(command, ImmutableNameSet.Create(names), null));
        }

        [NotNull]
        [PublicAPI]
        public static CommandCollection Add(this CommandCollection commandCollection, [NotNull] Action<object> action, [NotNull] params string[] names)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (names == null) throw new ArgumentNullException(nameof(names));
            if (!names.Any()) throw new ArgumentException(paramName: nameof(names), message: "You need to specify at least one name.");

            return commandCollection.Add(new Services.CommandExecutor(new SimpleCommand(action), ImmutableNameSet.Create(names), null));
        }

        [NotNull]
        private static CommandCollection Add(this CommandCollection commandCollection, Services.CommandExecutor commandExecutor)
        {
            if (commandCollection.ContainsKey(commandExecutor.Name))
            {
                throw new ArgumentException($"A command with the name {commandExecutor.Name} already exists.");
            }

            return new CommandCollection(commandCollection.Add(commandExecutor.Name, commandExecutor));
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

            if (commandCollection.TryGetValue(ImmutableNameSet.Create(name), out Services.CommandExecutor invoker))
            {
                return new CommandCollection(commandCollection.Add(ImmutableNameSet.DefaultCommandName, invoker));
            }

            throw new ArgumentException($"Command {name} is not added yet.");
        }
    }
}