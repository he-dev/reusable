using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Colin.Collections;
using Reusable.Colin.Data;

namespace Reusable.Colin.Commands
{
    public interface IProxyCommand : ICommand
    {
        [NotNull]
        ImmutableNameSet Name { get; }

        [NotNull]
        [ItemNotNull]
        IEnumerable<Data.ParameterInfo> Parameters { get; }
    }

    public class ProxyCommand : IProxyCommand
    {
        private readonly ICommand _command;

        private readonly ParameterFactory _parameterFactory;

        public ProxyCommand(ICommand command, Type parameterType, params string[] names)
        {
            _command = command;
            _parameterFactory = new ParameterFactory(parameterType);
            Name =
                names.Any()
                    ? ImmutableNameSet.Create(names)
                    : ImmutableNameSet.From(command);
        }

        public ProxyCommand(ProxyCommand command, ImmutableNameSet name)
        {
            _command = command._command;
            _parameterFactory = command._parameterFactory;
            Name = name;
        }

        public ImmutableNameSet Name { get; }

        public IEnumerable<Data.ParameterInfo> Parameters => _parameterFactory;

        public event EventHandler CanExecuteChanged;

        public static ProxyCommand Create(ICommand command, Type parameterType, params string[] names) => new ProxyCommand(command, parameterType, names);

        public static ProxyCommand Create(ICommand command, params string[] names) => new ProxyCommand(command, null, names);

        public bool CanExecute(object parameter) => _command.CanExecute(parameter);

        public void Execute(object parameter)
        {
            if (!(parameter is CommandLineContext commandLineContext)) { throw new ArgumentException($"The '{nameof(parameter)}' must be a '{nameof(CommandLineContext)}'."); }

            var commandParameter = _parameterFactory.CreateParameter(commandLineContext.Arguments);
            _command.Execute(new ExecuteContext(commandLineContext.CommandLine, commandParameter));
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
}