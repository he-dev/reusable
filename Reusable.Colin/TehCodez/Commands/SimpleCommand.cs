using System;
using System.Windows.Input;

namespace Reusable.Colin.Commands
{
    public class SimpleCommand : ICommand
    {
        private readonly Action<object> _execute;

        public SimpleCommand(Action<object> execute) => _execute = execute ?? throw new ArgumentNullException(nameof(execute));

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => _execute(parameter);
    }
}
