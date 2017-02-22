using System;
using System.Windows.Input;
using Reusable;
using System.Collections.Generic;
using Reusable.Shelly.Collections;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Immutable;

namespace Reusable.Shelly.Data
{
    public class ProxyCommand : ICommand
    {
        private readonly ICommand _command;
        private readonly ImmutableHashSet<string> _names;
        private readonly ParameterFactory _parameterFactory;

        public ProxyCommand(ICommand command, ImmutableHashSet<string> names, Type parameterType)
        {
            _command = command;
            _names = names;
            _parameterFactory = new ParameterFactory(parameterType, null);
        }

        public ProxyCommand(ICommand command)
            : this(command, CommandReflector.GetNames(command), null)
        { }

        public ProxyCommand(ICommand command, ImmutableHashSet<string> names)
            : this(command, names, null)
        { }

        public ProxyCommand(ICommand command, Type parameterType)
            : this(command, CommandReflector.GetNames(command.GetType()), parameterType)
        { }

        public ImmutableHashSet<string> Names => _names;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            switch (parameter)
            {
                case ImmutableHashSet<string> nameSet when _names.Overlaps(nameSet): return _command.CanExecute(parameter);
                default: return false;
            }
        }

        public void Execute(object parameter)
        {
            var context = (CommandLineContext)parameter;

            // TODO map from arguments to parameter object
            var arguments = (ArgumentCollection)context.Parameter;
            var commandParameter = _parameterFactory.CreateParameter(arguments);

            _command.Execute(commandParameter);
        }
    }
}