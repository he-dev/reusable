using System;
using System.Windows.Input;

namespace Reusable.CommandLine.Tests.Helpers
{
    class FooCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            throw new NotImplementedException();
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}