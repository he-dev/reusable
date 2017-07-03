using System;
using System.Windows.Input;

namespace Reusable.CommandLine.Commands
{
    public class LambdaCommand : ICommand
    {
        private readonly Action<object> _execute;

        public LambdaCommand(Action<object> execute) => _execute = execute ?? throw new ArgumentNullException(nameof(execute));

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => _execute(parameter);
    }
}
