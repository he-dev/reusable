using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Shelly.Data;
using Reusable.Shelly.Reflection;
using Reusable.Shelly.Collections;
using Reusable.Shelly.Commands;
using System.Windows.Input;
using Reusable.TypeConversion;
using System.Reflection;

namespace Reusable.Shelly
{
    public class CommandLineBuilder
    {
        private readonly CommandCollection _commands = new CommandCollection();
        private char _argumentPrefix = '-';
        private char _argumentValueSeparator = ':';
        private Action<string> _log = s => { };
        private TypeConverter _converter = TypeConverter.Empty;

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

        private CommandLineBuilder Register(ICommand command) { _commands.Add(command); return this; }

        public CommandLineBuilder Register<TCommand, TParameters>()
            where TCommand : ICommand, new()
            where TParameters : new()
            => Register(new ProxyCommand(new TCommand(), typeof(TParameters)));

        public CommandLineBuilder Register<TCommand>()
            where TCommand : ICommand, new()
            => Register(new ProxyCommand(new TCommand()));

        public CommandLineBuilder Register(string[] names, Action<object> excecute)
            => Register(new ProxyCommand(new SimpleCommand(excecute), ImmutableNameSet.Create(names)));

        public CommandLineBuilder AsDefault()
        {
            if (_commands.Any(x => x is DefaultCommand)) throw new InvalidOperationException("There is already a default command.");
            var command = _commands.LastOrDefault() ?? throw new InvalidOperationException("There are no registered commands.");
            return Register(new DefaultCommand(command));
        }

        public CommandLineBuilder Log(Action<string> log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            return this;
        }

        public CommandLineBuilder Converter(Func<TypeConverter, TypeConverter> converter)
        {
            return this;
        }

        public CommandLine Build() => new CommandLine(
            _commands,
            _argumentPrefix,
            _argumentValueSeparator,
            _log
        );
    }

    public static class CommandLineBuilderExtensions
    {
        public static CommandLineBuilder RegisterHelpCommand(this CommandLineBuilder builder) => builder.Register<HelpCommand, HelpCommandParameters>(); 
    }
}