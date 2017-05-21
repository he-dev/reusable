using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Colin.Collections;
using Reusable.Colin.Commands;
using Reusable.Colin.Data;

namespace Reusable.Colin
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

            ParameterFactory = new ParameterFactory(parameterType);
        }

        public ICommand Command { get; }
        public ImmutableNameSet Name { get; }
        public ParameterFactory ParameterFactory { get; }

        public void Invoke(CommandLine commandLine, ArgumentLookup argument)
        {
            var commandParameter = ParameterFactory.CreateParameter(argument);
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

        public CommandLine CommandLine { get; }

        public object Parameter { get; }
    }

    public class CommandLineBuilder
    {
        private readonly IDictionary<ImmutableNameSet, CommandInvoker> _commands = new Dictionary<ImmutableNameSet, CommandInvoker>(ImmutableNameSet.Comparer);
        private char _argumentPrefix = '-';
        private char _argumentValueSeparator = ':';
        private Action<string> _log = s => { };
        private CommandInvoker _lastCommandInvoker;

        internal CommandLineBuilder() { }

        public CommandLineBuilder ArgumentPrefix(char argumentPrefix)
        {
            _argumentPrefix = argumentPrefix;
            return this;
        }

        public CommandLineBuilder ArgumentValueSeparator(char argumentValueSeparator)
        {
            _argumentValueSeparator = argumentValueSeparator;
            return this;
        }

        [NotNull]
        private CommandLineBuilder Register(CommandInvoker commandInvoker)
        {
            if (_commands.ContainsKey(commandInvoker.Name)) { throw new DuplicateCommandNameException(commandInvoker.Name); }
            _commands.Add(commandInvoker.Name, commandInvoker);
            if (commandInvoker.Name != CommandLine.DefaultCommandName)
            {
                _lastCommandInvoker = commandInvoker;
            }
            return this;
        }

        [NotNull]
        public CommandLineBuilder Register<TCommand, TParameter>()
            where TCommand : ICommand, new()
            where TParameter : new()
        {
            return Register(new CommandInvoker(new TCommand(), ImmutableNameSet.Empty, typeof(TParameter)));
        }

        [NotNull]
        public CommandLineBuilder Register<TCommand>()
            where TCommand : ICommand, new()
        {
            return Register(new CommandInvoker(new TCommand(), ImmutableNameSet.Empty, null));
        }

        [NotNull]
        public CommandLineBuilder Register<TParameter>(ICommand command, params string[] names)
            where TParameter : new()
        {
            return Register(new CommandInvoker(command, ImmutableNameSet.Create(names), typeof(TParameter)));
        }

        [NotNull]
        public CommandLineBuilder Register(ICommand command, params string[] names)
        {
            return Register(new CommandInvoker(command, ImmutableNameSet.Create(names), null));
        }

        [NotNull]
        public CommandLineBuilder Register([NotNull] Action<object> action, [NotNull] params string[] names)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (names == null) throw new ArgumentNullException(nameof(names));
            if (!names.Any()) throw new ArgumentException(paramName: nameof(names), message: "You need to specify at least one name.");

            return Register(new CommandInvoker(new SimpleCommand(action), ImmutableNameSet.Create(names), null));
        }

        [NotNull]
        public CommandLineBuilder AsDefault()
        {
            if (_commands.ContainsKey(CommandLine.DefaultCommandName)) { throw new InvalidOperationException("There is already a default command. Only one command can be default."); }
            if (_lastCommandInvoker == null) { throw new InvalidOperationException("There are no registered commands. You need to register at least one command."); }


            return Register(
                new CommandInvoker(
                    _lastCommandInvoker.Command,
                    CommandLine.DefaultCommandName,
                    _lastCommandInvoker.ParameterFactory.ParameterType));
        }

        [NotNull]
        public CommandLineBuilder Log([NotNull] Action<string> log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            return this;
        }

        public CommandLine Build() => new CommandLine(_argumentPrefix, _argumentValueSeparator, _log, _commands.ToImmutableDictionary(ImmutableNameSet.Comparer));
    }

    public static class CommandLineBuilderExtensions
    {
        public static CommandLineBuilder RegisterHelpCommand(this CommandLineBuilder builder) => builder.Register<HelpCommand, HelpCommandParameter>();
    }
}