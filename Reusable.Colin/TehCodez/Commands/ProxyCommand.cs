using System;
using System.Linq;
using System.Windows.Input;
using Reusable.Colin.Collections;
using Reusable.Colin.Data;

namespace Reusable.Colin.Commands
{
    public class ProxyCommand : ICommand
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

        public ImmutableNameSet Name { get; }

        public event EventHandler CanExecuteChanged;

        public static ProxyCommand Create(ICommand command, Type parameterType, params string[] names) => new ProxyCommand(command, parameterType, names);

        public static ProxyCommand Create(ICommand command, params string[] names) => new ProxyCommand(command, null, names);

        public bool CanExecute(object parameter) => _command.CanExecute(parameter);

        public void Execute(object parameter)
        {
            if (!(parameter is CommandLineContext ctx)) throw new ArgumentException($"The '{nameof(parameter)}' must be a '{nameof(CommandLineContext)}'.");

            var commandParameter = _parameterFactory.CreateParameter(ctx.Arguments);
            _command.Execute(commandParameter);
        }
    }
}