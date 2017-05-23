using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using Reusable.Colin.Annotations;
using Reusable.Colin.Collections;
using Reusable.Colin.Services;
using Reusable.Colin.Tests.Helpers;

namespace Reusable.Colin.Tests
{
    [TestClass]
    public class CommandLineTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Add_DuplicateCommands_Throws()
        {
            new CommandLine().Add<TestCommand>().Add<TestCommand>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Add_DefaultWithEmptyCommandLine_Throws()
        {
            new CommandLine().Default("test");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Add_TwoDefaultCommands_Throws()
        {
            new CommandLine().Add<TestCommand>().Default("test").Default("test");
        }

        [TestMethod]
        public void Execute_EmptyCommandLine_NoCommandExecuted()
        {
            var testCmd = new TestCommand();
            var cmdLn = new CommandLine().Add(testCmd).Default("test");
            cmdLn.Execute("");
            Assert.AreEqual(0, testCmd.Parameters.Count);
        }

        [TestMethod]
        public void Execute_CommandLineWithName_ExecutedCommandByName()
        {
            var testCmd1 = new TestCommand();
            var testCmd2 = new TestCommand();

            var cmdLn = new CommandLine()
                .Add<object>(testCmd1, "test1", "t1")
                .Add<object>(testCmd2, "test2", "t2");
            cmdLn.Execute("t2");

            testCmd1.Parameters.Count.Verify().IsEqual(0);
            testCmd2.Parameters.Count.Verify().IsEqual(1);
        }

        [TestMethod]
        public void Execute_CommandWithParameters_Executed()
        {
            var testCmd = new TestCommand();
            var cmdLn = new CommandLine()
                .Add<TestParameter>(testCmd);

            cmdLn.Execute("test -foo:oof -bar:3 -arr: 4 5 6 -flag1 -flag2:false");

            Assert.AreEqual(1, testCmd.Parameters.Count);

            var param = testCmd.Parameters.OfType<TestParameter>().Single();

            Assert.AreEqual("oof", param.Foo);
            Assert.AreEqual(3, param.Bar);
            Assert.AreEqual(1.5, param.Baz);
            CollectionAssert.AreEqual(new[] { 4, 5, 6 }, param.Arr);
        }
    }
}

