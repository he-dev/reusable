using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Reusable.CommandLine.Annotations;
using Reusable.CommandLine.Data;

namespace Reusable.CommandLine.Tests.Helpers
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

        [Parameter(AllowShortName = false)]
        public int Bar { get; set; }

        [Parameter(AllowShortName = false)]
        [DefaultValue(1.5)]
        public double Baz { get; set; }

        [Parameter(AllowShortName = false)]
        public int[] Arr { get; set; }

        [Parameter(AllowShortName = false)]
        [DefaultValue(true)]
        public bool Flag1 { get; set; }

        [Parameter(AllowShortName = false)]
        [DefaultValue(true)]
        public bool Flag2 { get; set; }
    }
}