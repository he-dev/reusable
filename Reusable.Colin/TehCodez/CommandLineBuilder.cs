using System;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Colin.Collections;
using Reusable.Colin.Commands;
using Reusable.Colin.Services;

namespace Reusable.Colin
{
    public static class CommandLineBuilder
    {
        [NotNull]
        [PublicAPI]
        public static CommandLine Add<TCommand, TParameter>(this CommandLine commandLine)
            where TCommand : ICommand, new()
            where TParameter : new()
        {
            return commandLine.Add(new CommandExecutor(new TCommand(), ImmutableNameSet.Empty, typeof(TParameter)));
        }

        [NotNull]
        [PublicAPI]
        public static CommandLine Add<TCommand>(this CommandLine commandLine)
            where TCommand : ICommand, new()
        {
            return commandLine.Add(new CommandExecutor(new TCommand(), ImmutableNameSet.Empty, null));
        }

        [NotNull]
        [PublicAPI]
        public static CommandLine Add<TParameter>(this CommandLine commandLine, ICommand command, params string[] names)
            where TParameter : new()
        {
            return commandLine.Add(new CommandExecutor(command, ImmutableNameSet.Create(names), typeof(TParameter)));
        }

        [NotNull]
        [PublicAPI]
        public static CommandLine Add(this CommandLine commandLine, ICommand command, params string[] names)
        {
            return commandLine.Add(new CommandExecutor(command, ImmutableNameSet.Create(names), null));
        }

        [NotNull]
        [PublicAPI]
        public static CommandLine Add(this CommandLine commandLine, [NotNull] Action<object> action, [NotNull] params string[] names)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (names == null) throw new ArgumentNullException(nameof(names));
            if (!names.Any()) throw new ArgumentException(paramName: nameof(names), message: "You need to specify at least one name.");

            return commandLine.Add(new CommandExecutor(new SimpleCommand(action), ImmutableNameSet.Create(names), null));
        }

        [NotNull]
        private static CommandLine Add(this CommandLine commandLine, CommandExecutor commandExecutor)
        {
            if (commandLine.ContainsKey(commandExecutor.Name))
            {
                throw new ArgumentException($"A command with the name {commandExecutor.Name} already exists.");
            }

            return new CommandLine(commandLine.Add(commandExecutor.Name, commandExecutor));
        }

        [NotNull]
        [PublicAPI]
        public static CommandLine Default(this CommandLine commandLine, string name)
        {
            if (commandLine.ContainsKey(CommandLine.DefaultCommandName))
            {
                throw new ArgumentException("Default command is already added. There can be only one default command.");
            }

            if (commandLine.TryGetValue(ImmutableNameSet.Create(name), out CommandExecutor invoker))
            {
                return new CommandLine(commandLine.Add(CommandLine.DefaultCommandName, invoker));
            }

            throw new ArgumentException($"Command {name} is not added yet.");
        }
    }
}