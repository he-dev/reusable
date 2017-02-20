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
        private readonly CommandParameterCollection _parameters;        


        public ProxyCommand(ICommand command, ImmutableHashSet<string> names, CommandParameterCollection parameters)
        {
            _command = command;
            _names = names;
            _parameters = parameters;
        }

        public ProxyCommand(ICommand command)
            : this(
                  command, 
                  Command.GetNames(command.GetType()), null
            )
        { }

        public ProxyCommand(ICommand command, ImmutableHashSet<string> names) 
            : this(
                  command, 
                  names, 
                  null
            )
        { }

        public ProxyCommand(ICommand command, Type parameterType)
            : this(
                  command,
                  Command.GetNames(command.GetType()),
                  new CommandParameterCollection(parameterType)
            )
        { }

        public ImmutableHashSet<string> Names => _names;

        public CommandParameterCollection Parameters => _parameters;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            switch (parameter)
            {
                case StringSet nameSet when _names.Overlaps(nameSet): return _command.CanExecute(parameter);
                default: return false;
            }
        }

        public void Execute(object parameter)
        {
            var context = (CommandLineContext)parameter;

            // TODO map from arguments to parameter object
            var arguments = (ArgumentCollection)context.Parameter;

            _command.Execute(null);
        }
    }
}