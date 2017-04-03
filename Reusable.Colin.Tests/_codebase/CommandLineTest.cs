using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using Reusable.Colin.Data;
using System.Windows.Input;

namespace Reusable.Colin.Tests
{
    [TestClass]
    public class CommandLineTest
    {
        [TestMethod]
        public void Execute_EmptyCommandLine_DefaultCommand()
        {
            var executed = false;
            var execute = new Action<object>(o => { executed = true; });
            var cmdLn = CommandLine.Builder.Register(execute, "test").AsDefault().Build();
            cmdLn.Execute("");
            executed.Verify().IsTrue();
        }

        [TestMethod]
        public void Execute_CommandLineWithName_SelectedCommand()
        {
            var executed = new bool[2];
            var execute = new Action<object>[]
            {
                o => { executed[0] = true; },
                o => { executed[1] = true; }
            };

            var cmdLn = CommandLine.Builder
                .Register(execute[0], "test1", "t1")
                .Register(execute[1], "test2", "t2")
                .Build();
            cmdLn.Execute("t2");

            executed[0].Verify().IsFalse();
            executed[1].Verify().IsTrue();
        }

        [TestMethod]
        public void Execute_CommandWithParameters_Executed()
        {
            var cmdLn = CommandLine.Builder
                .Register<FooCommand>()
                .Register<BarCommand, BarParameters>()
                .Register<BazCommand>()
                .ArgumentPrefix('-')
                .ArgumentValueSeparator('=')
                .Build();

            cmdLn.Execute("bar -requiredparameter=abc");
        }

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

        class BazCommand : ICommand
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
}
