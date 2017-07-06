using System;
using System.Windows.Input;

namespace Reusable.CommandLine.Tests.Commnads
{
    internal abstract class TestCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public virtual bool CanExecute(object parameter) => throw new NotImplementedException();

        public virtual void Execute(object parameter) => throw new NotImplementedException();
    }
}
