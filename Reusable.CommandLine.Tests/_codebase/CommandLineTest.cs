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
    }
}
