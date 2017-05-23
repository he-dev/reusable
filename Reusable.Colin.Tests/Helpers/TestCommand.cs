using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Reusable.Colin.Annotations;
using Reusable.Colin.Data;

namespace Reusable.Colin.Tests.Helpers
{
    class TestCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public IList<object> Parameters { get; } = new List<object>();

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            Parameters.Add((parameter as CommandContext).Parameter);
        }
    }

    class TestParameter
    {
        [Parameter]
        public string Foo { get; set; }

        [Parameter(CanCreateShortName = false)]
        public int Bar { get; set; }

        [Parameter(CanCreateShortName = false)]
        [DefaultValue(1.5)]
        public double Baz { get; set; }

        [Parameter(CanCreateShortName = false)]
        public int[] Arr { get; set; }

        [Parameter(CanCreateShortName = false)]
        [DefaultValue(true)]
        public bool Flag1 { get; set; }

        [Parameter(CanCreateShortName = false)]
        [DefaultValue(true)]
        public bool Flag2 { get; set; }
    }
}