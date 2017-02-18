using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Reusable.Shelly.Commands
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
