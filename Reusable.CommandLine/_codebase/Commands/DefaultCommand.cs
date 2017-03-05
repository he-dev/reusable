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
    public class DefaultCommand : ICommand
    {
        private readonly ICommand _command;

        public static readonly ImmutableHashSet<string> NameSet = ImmutableNameSet.Create("Default");

        public event EventHandler CanExecuteChanged;

        public DefaultCommand(ICommand command)
        {
            _command = command;
        }

        public bool CanExecute(object parameter)
        {
            switch (parameter)
            {
                case ImmutableHashSet<string> nameSet when NameSet.Overlaps(nameSet): return true;
                default: return false;
            }
        }

        public void Execute(object parameter) => _command.Execute(parameter);
    }
}