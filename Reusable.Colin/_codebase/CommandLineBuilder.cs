using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Reusable.Colin.Collections;
using Reusable.Colin.Commands;
using Reusable.Colin.Data;
using Reusable.TypeConversion;

namespace Reusable.Colin
{
    public class CommandLineBuilder
    {
        private readonly HashSet<ICommand> _commands = new HashSet<ICommand>(new ProxyCommandComparer());
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

        private CommandLineBuilder Register(ICommand command)
        {
            if (!_commands.Add(command)) throw new DuplicateCommandNameException((command as ProxyCommand).Names);
            return this;
        }

        public CommandLineBuilder Register<TCommand, TParameters>()
            where TCommand : ICommand, new()
            where TParameters : new()
            => Register(new ProxyCommand(new TCommand(), typeof(TParameters)));

        public CommandLineBuilder Register<TCommand>()
            where TCommand : ICommand, new()
            => Register(new ProxyCommand(new TCommand()));

        public CommandLineBuilder Register(Action<object> execute, params string[] names)
            => Register(new ProxyCommand(new SimpleCommand(execute), ImmutableNameSet.Create(names)));

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

    internal class ProxyCommandComparer : IEqualityComparer<ICommand>
    {
        public bool Equals(ICommand x, ICommand y)
        {
            return
                !ReferenceEquals(x, null) &&
                !ReferenceEquals(y, null) &&
                (x is ProxyCommand pc1) &&
                (y is ProxyCommand pc2) &&
                pc1.Names.Overlaps(pc2.Names);
        }

        public int GetHashCode(ICommand obj) => 0;
    }

    public static class CommandLineBuilderExtensions
    {
        public static CommandLineBuilder RegisterHelpCommand(this CommandLineBuilder builder) => builder.Register<HelpCommand, HelpCommandParameters>();
    }
}