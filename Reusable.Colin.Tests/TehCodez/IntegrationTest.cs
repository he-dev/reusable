using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.CommandLine.Tests.Commnads;
using Reusable.CommandLine.Tests.Data;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using Reusable.TestTools.UnitTesting.AssertExtensions.TehCodez;
using FooCommand = Reusable.CommandLine.Tests.Commnads.FooCommand;

namespace Reusable.CommandLine.Tests
{
    [TestClass]
    public class IntegrationTest
    {
        [TestMethod]
        public void Execute_EmptyString_EmptyContainer_DoesNothing()
        {
            var cmdLn = CommandContainer.Empty;
            Assert.IsFalse(string.Empty.Execute(cmdLn));
        }

        [TestMethod]
        public void Execute_EmptyString_OneCommand_Executed()
        {
            var cmd = new FooCommand();
            var cmdLn = CommandContainer.Empty.Add(cmd);
            Assert.IsTrue(string.Empty.Execute(cmdLn));
            Assert.That.CountEquals(1, cmd.ExecuteLog);
        }

        [TestMethod]
        public void Execute_EmptyString_TwoCommands_Throws()
        {
            var cmdLn = CommandContainer.Empty.Add<FooCommand>().Add<BarCommand>();
            Assert.IsFalse(string.Empty.Execute(cmdLn));
        }

        [TestMethod]
        public void Execute_CommandLineWithName_ExecutedCommandByName()
        {
            var fooCmd = new FooCommand();
            var barCmd = new BarCommand();

            var cmdLn = CommandContainer.Empty
                .Add<object>(fooCmd, "test1", "t1")
                .Add<object>(barCmd, "test2", "t2");
            //cmdLn.Execute("t2");

            fooCmd.ExecuteLog.Count.Verify().IsEqual(0);
            barCmd.ExecuteLog.Count.Verify().IsEqual(1);
        }

        [TestMethod]
        public void Execute_CommandWithParameters_Executed()
        {
            var cmd = new FooCommand();
            var cmdLn = CommandContainer.Empty
                .Add<TestParameter>(cmd);

            "-foo:oof -bar:3 -arr: 4 5 6 -flag1 -flag2:false".Execute(cmdLn);

            Assert.AreEqual(1, cmd.ExecuteLog.Count);

            var param = cmd.ExecuteLog.OfType<TestParameter>().Single();

            Assert.AreEqual("oof", param.Foo);
            Assert.AreEqual(3, param.Bar);
            Assert.AreEqual(1.5, param.Baz);
            CollectionAssert.AreEqual(new[] { 4, 5, 6 }, param.Arr);
        }
    }
}

