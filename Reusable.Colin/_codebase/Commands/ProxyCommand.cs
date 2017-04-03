using System;
using System.Windows.Input;
using Reusable;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Immutable;
using Reusable.Colin.Collections;
using Reusable.Colin.Data;

namespace Reusable.Colin.Commands
{
    public class ProxyCommand : ICommand
    {
        private readonly ICommand _command;
        private readonly ImmutableNameSet _names;
        private readonly ParameterFactory _parameterFactory;

        public ProxyCommand(ICommand command, ImmutableNameSet names, Type parameterType)
        {
            _command = command;
            _names = names;
            _parameterFactory = new ParameterFactory(parameterType);
        }

        public ProxyCommand(ICommand command)
            : this(command, CommandReflector.GetNames(command), null)
        { }

        public ProxyCommand(ICommand command, ImmutableNameSet names)
            : this(command, names, null)
        { }

        public ProxyCommand(ICommand command, Type parameterType)
            : this(command, CommandReflector.GetNames(command.GetType()), parameterType)
        { }

        public ImmutableNameSet Names => _names;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            switch (parameter)
            {
                case ImmutableNameSet nameSet when _names.Overlaps(nameSet): return _command.CanExecute(parameter);
                default: return false;
            }
        }

        public void Execute(object parameter)
        {
            var context = (CommandLineContext)parameter;
            var commandParameter = _parameterFactory.CreateParameter(context.Arguments);
            _command.Execute(commandParameter);
        }
    }
}