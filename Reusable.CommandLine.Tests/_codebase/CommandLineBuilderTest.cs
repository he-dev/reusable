using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using Reusable.Shelly.Data;
using System.Windows.Input;

namespace Reusable.Shelly.Tests
{
    [TestClass]
    public class CommandLineBuilderTest
    {
        [TestMethod]
        public void Register_SingleCommand_DefaultCommandLine()
        {
            var executed = false;
            var execute = new Action<object>(o => { executed = true; });
            var cmdLn = CommandLine.Builder.Register(new[] { "test" }, execute).Build();
            cmdLn.Execute("-a");
            executed.Verify().IsTrue();
        }

        private class TestCommand : ICommand
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

        private class Test2Command : ICommand
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
