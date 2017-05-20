using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using Reusable.Colin.Data;
using System.Windows.Input;
using Reusable.Colin.Annotations;

namespace Reusable.Colin.Tests
{
    [TestClass]
    public class CommandLineTest
    {
        [TestMethod]
        public void Execute_EmptyCommandLine_NoCommandExecuted()
        {
            var testCmd = new TestCommand();
            var cmdLn = CommandLine.Builder.Register<object>(testCmd).AsDefault().Build();
            cmdLn.Execute("");
            Assert.AreEqual(0, testCmd.Parameters.Count);
        }

        [TestMethod]
        public void Execute_CommandLineWithName_ExecutedCommandByName()
        {
            var testCmd1 = new TestCommand();
            var testCmd2 = new TestCommand();            

            var cmdLn = CommandLine.Builder
                .Register<object>(testCmd1, "test1", "t1")
                .Register<object>(testCmd2, "test2", "t2")
                .Build();
            cmdLn.Execute("t2");

            testCmd1.Parameters.Count.Verify().IsEqual(0);
            testCmd2.Parameters.Count.Verify().IsEqual(1);
        }

        [TestMethod]
        public void Execute_CommandWithParameters_ExecutedCommandWithParameters()
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

        class TestCommand : ICommand
        {
            public event EventHandler CanExecuteChanged;

            public IList<object> Parameters { get; } = new List<object>();

            public bool CanExecute(object parameter) => true;

            public void Execute(object parameter)
            {
                Parameters.Add(parameter);
            }
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
