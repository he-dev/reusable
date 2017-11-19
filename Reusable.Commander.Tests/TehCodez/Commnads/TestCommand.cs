using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Reusable.Commander.Tests.Commnads
{
    internal abstract class TestCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public IList<object> ExecuteLog { get; } = new List<object>();

        public bool CanExecute(object parameter) => true;

        public virtual void Execute(object parameter)
        {
            ExecuteLog.Add(parameter);
        }
    }

    internal class FooCommand : TestCommand { }

    internal class BarCommand : TestCommand { }
}