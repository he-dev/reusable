using System;
using System.Windows.Input;
using Reusable;
using System.Collections.Generic;
using Reusable.Shelly.Collections;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;

namespace Reusable.Shelly.Data
{
    public class DefaultCommand : ICommand
    {
        private readonly ICommand _command;

        public static readonly StringSet NameSet = StringSet.CreateCI("default");

        public event EventHandler CanExecuteChanged;

        public DefaultCommand(ICommand command)
        {
            _command = command;
        }

        public bool CanExecute(object parameter)
        {
            switch (parameter)
            {
                case StringSet nameSet when NameSet.Overlaps(nameSet): return true;
                default: return false;
            }
        }

        public void Execute(object parameter) => _command.Execute(parameter);
    }
}