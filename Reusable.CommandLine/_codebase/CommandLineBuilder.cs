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
        private CommandInfo _helpCommand = new CommandInfo(new HelpCommand(), typeof(HelpCommandParameters));
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

        private CommandLineBuilder Register(CommandInfo command) { _commands.Add(command); return this; }

        //private CommandLineBuilder Register(ICommand command, string[] names, Type parameterType)
        //{
        //    _commands.Add(
        //        command ?? throw new ArgumentNullException(nameof(command)),
        //        StringSetCI.Create(names),
        //        new CommandParameterCollection(parameterType)
        //    );
        //    return this;
        //}

        public CommandLineBuilder Register<TCommand, TParameters>()
            where TCommand : ICommand, new()
            where TParameters : new()
            => Register(new CommandInfo(new TCommand(), typeof(TParameters)));

        public CommandLineBuilder Register<TCommand>()
            where TCommand : ICommand, new()
            => Register(new CommandInfo(new TCommand()));

        public CommandLineBuilder Register(string[] names, Action<object> excecute)
            => Register(new CommandInfo(new SimpleCommand(excecute), StringSetCI.Create(names)));

        public CommandLineBuilder Help<TCommand>() where TCommand : ICommand, new()
        {
            _helpCommand = new CommandInfo(new TCommand(), typeof(HelpCommandParameters));
            return this;
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
            _helpCommand,
            _log
        );
    }
}