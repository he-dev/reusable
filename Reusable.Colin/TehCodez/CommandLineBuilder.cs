using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Reusable.Colin.Collections;
using Reusable.Colin.Services;

namespace Reusable.Colin
{
    public class CommandLineBuilder
    {
        private readonly IDictionary<ImmutableNameSet, CommandInvoker> _commands = new Dictionary<ImmutableNameSet, CommandInvoker>(ImmutableNameSet.Comparer);
        private char _argumentPrefix = '-';
        private char _argumentValueSeparator = ':';
        private Action<string> _log = m => { };
        private CommandInvoker _lastCommandInvoker;

        internal CommandLineBuilder() { }

        [NotNull]
        public CommandLineBuilder ArgumentPrefix(char argumentPrefix)
        {
            _argumentPrefix = argumentPrefix;
            return this;
        }

        [NotNull]
        public CommandLineBuilder ArgumentValueSeparator(char argumentValueSeparator)
        {
            _argumentValueSeparator = argumentValueSeparator;
            return this;
        }
        
        [NotNull]
        public CommandLineBuilder Register(CommandInvoker commandInvoker)
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
        public CommandLineBuilder AsDefault()
        {
            if (_commands.ContainsKey(CommandLine.DefaultCommandName)) { throw new InvalidOperationException("There is already a default command. Only one command can be default."); }
            if (_lastCommandInvoker == null) { throw new InvalidOperationException("There are no registered commands. You need to register at least one command."); }

            return Register(
                new CommandInvoker(
                    _lastCommandInvoker.Command,
                    CommandLine.DefaultCommandName,
                    _lastCommandInvoker.CommandParameterFactory.ParameterType));
        }

        [NotNull]
        public CommandLineBuilder Log([NotNull] Action<string> log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            return this;
        }

        public CommandLine Build() => new CommandLine(_argumentPrefix, _argumentValueSeparator, _log, _commands.ToImmutableDictionary(ImmutableNameSet.Comparer));
    }
}