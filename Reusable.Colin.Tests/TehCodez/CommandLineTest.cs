using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
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
            CommandCollection.Empty.Add<TestCommand>().Add<TestCommand>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Add_DefaultWithEmptyCommandLine_Throws()
        {
            CommandCollection.Empty.Default("test");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Add_TwoDefaultCommands_Throws()
        {
            var testCmd1 = new TestCommand();
            var testCmd2 = new TestCommand();
            CommandCollection.Empty
                .Add<object>(testCmd1, "test1", "t1")
                .Add<object>(testCmd2, "test2", "t2")
                .Default("test")
                .Default("test");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Add_SingleCommandAsDefault_Throws()
        {
            CommandCollection.Empty.Add<TestCommand>().Default("test");
        }

        [TestMethod]
        public void Execute_EmptyCommandLine_DefaultCommandExecuted()
        {
            var testCmd = new TestCommand();
            var cmdLn = CommandCollection.Empty.Add(testCmd);
            cmdLn.Execute("");
            Assert.AreEqual(1, testCmd.Parameters.Count);
        }

        [TestMethod]
        public void Execute_CommandLineWithName_ExecutedCommandByName()
        {
            var testCmd1 = new TestCommand();
            var testCmd2 = new TestCommand();

            var cmdLn = CommandCollection.Empty
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
            var cmdLn = CommandCollection.Empty
                .Add<TestParameter>(testCmd);

            cmdLn.Execute("-foo:oof -bar:3 -arr: 4 5 6 -flag1 -flag2:false");

            Assert.AreEqual(1, testCmd.Parameters.Count);

            var param = testCmd.Parameters.OfType<TestParameter>().Single();

            Assert.AreEqual("oof", param.Foo);
            Assert.AreEqual(3, param.Bar);
            Assert.AreEqual(1.5, param.Baz);
            CollectionAssert.AreEqual(new[] { 4, 5, 6 }, param.Arr);
        }
    }
}

