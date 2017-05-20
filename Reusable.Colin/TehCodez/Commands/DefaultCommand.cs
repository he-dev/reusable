using System;
using System.Windows.Input;
using Reusable;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Immutable;
using Reusable.Colin.Collections;

namespace Reusable.Colin.Commands
{
    public class DefaultCommand : ICommand
    {
        private readonly ICommand _command;

        public static readonly ImmutableNameSet Name = ImmutableNameSet.Create("Default");

        public event EventHandler CanExecuteChanged;

        public DefaultCommand(ICommand command)
        {
            _command = command;
        }

        public bool CanExecute(object parameter) => _command.CanExecute(parameter);

        public void Execute(object parameter) => _command.Execute(parameter);
    }
}