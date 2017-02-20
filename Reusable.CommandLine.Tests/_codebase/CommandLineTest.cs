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
        public void Execute_NoArgs_DefaultCommand()
        {
            var executed = false;
            var execute = new Action<object>(o => { executed = true; });
            var cmdLn = CommandLine.Builder.Register(new[] { "test" }, execute).AsDefault().Build();
            cmdLn.Execute("");
            executed.Verify().IsTrue();
        }

        [TestMethod]
        public void Execute_CommandName_SelectedCommand()
        {
            var executedT1 = false;
            var executedT2 = false;
            var executeT1 = new Action<object>(o => { executedT1 = true; });
            var executeT2 = new Action<object>(o => { executedT2 = true; });
            var cmdLn = CommandLine.Builder
                .Register(new[] { "test1", "t1" }, executeT1)
                .Register(new[] { "test2", "t2" }, executeT2)
                .Build();
            cmdLn.Execute("t2");
            executedT1.Verify().IsFalse();
            executedT2.Verify().IsTrue();
        }
    }
}
