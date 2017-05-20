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
    public class CommandLineBuilder
    {
        private readonly IDictionary<ImmutableNameSet, ICommand> _commands = new Dictionary<ImmutableNameSet, ICommand>(ImmutableNameSet.Comparer);
        private char _argumentPrefix = '-';
        private char _argumentValueSeparator = ':';
        private Action<string> _log = s => { };
        private ProxyCommand _lastCommand;

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
        private CommandLineBuilder Register(ICommand command)
        {
            switch (command)
            {
                case ProxyCommand c:
                    if (_commands.ContainsKey(c.Name)) { throw new DuplicateCommandNameException(c.Name); }
                    _commands.Add(c.Name, (_lastCommand = c));
                    break;
                case DefaultCommand c:
                    _commands.Add(DefaultCommand.Name, c);
                    break;
            }
            return this;
        }

        [NotNull]
        public CommandLineBuilder Register<TCommand, TParameter>()
            where TCommand : ICommand, new()
            where TParameter : new()
        {
            return Register(new ProxyCommand(new TCommand(), typeof(TParameter)));
        }

        [NotNull]
        public CommandLineBuilder Register<TCommand>()
            where TCommand : ICommand, new()
        {
            return Register(ProxyCommand.Create(new TCommand()));
        }

        [NotNull]
        public CommandLineBuilder Register<TParameter>(ICommand command, params string[] names)
            where TParameter : new()
        {
            return Register(new ProxyCommand(command, typeof(TParameter), names));
        }

        [NotNull]
        public CommandLineBuilder Register([NotNull] Action<object> action, [NotNull] params string[] names)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (names == null) throw new ArgumentNullException(nameof(names));
            if (!names.Any()) throw new ArgumentException(paramName: nameof(names), message: "You need to specify at least one name.");

            return Register(ProxyCommand.Create(new SimpleCommand(action), names));
        }

        [NotNull]
        public CommandLineBuilder AsDefault()
        {
            if (_commands.ContainsKey(DefaultCommand.Name)) { throw new InvalidOperationException("There is already a default command. Only one command can be default."); }
            if (_lastCommand == null) { throw new InvalidOperationException("There are no registered commands. You need to register at least one command."); }

            return Register(new DefaultCommand(_lastCommand));
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
        public static CommandLineBuilder RegisterHelpCommand(this CommandLineBuilder builder) => builder.Register<HelpCommand, HelpCommandParameters>();
    }
}