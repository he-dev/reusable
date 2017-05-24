using System;
using System.Windows.Input;
using Reusable.Colin.Annotations;

namespace Reusable.Colin.Tests.Helpers
{
    class BarCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            var bar = parameter as BarParameters;
        }
    }

    class BarParameters
    {
        [Parameter(Required = true)]
        public string RequiredParameter { get; set; }

        [Parameter]
        public string OptionalParameter { get; set; }
    }
}