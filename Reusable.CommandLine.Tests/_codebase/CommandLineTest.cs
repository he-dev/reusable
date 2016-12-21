using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.Shelly.Tests
{
    [TestClass]
    public class CommandLineTest
    {
        [TestMethod]
        public void Execute_DefaultCommand()
        {
            var builder = new CommandLineBuilder();
            builder.Register<Test1Command>(new Action<Command>(cmd => {})).AsDefault();
            var cmdLine = builder.Build();

            cmdLine.Execute(Enumerable.Empty<string>());
        }

        [TestMethod]
        public void Execute_Throws_DefaultCommandNotFound()
        {
            new Action(() =>
            {
                var builder = new CommandLineBuilder();
                builder.Register<Test1Command>();
                var cmdLine = builder.Build();
                cmdLine.Execute(Enumerable.Empty<string>());
            })
            .Verify().Throws<CommnadNotFoundException>();
        }

        [TestMethod]
        public void Execute_CommandByName()
        {
            var test1Executed = false;
            var test2Executed = false;

            var builder = new CommandLineBuilder();
            builder.Register<Test1Command>(new Action<Command>(cmd => { test1Executed = true; }));
            builder.Register<Test2Command>(new Action<Command>(cmd => { test2Executed = true; }));
            var cmdLine = builder.Build();
            cmdLine.Execute(new[] { "test2" });

            test1Executed.Verify(nameof(test1Executed)).IsFalse();
            test2Executed.Verify(nameof(test2Executed)).IsTrue();
        }

        private class Test1Command : TestCommand
        {
            public Test1Command(Action<Command> execute) : base(execute) { }
        }

        private class Test2Command : TestCommand
        {
            public Test2Command(Action<Command> execute) : base(execute) { }
        }
    }

    internal class TestCommand : Command
    {
        private readonly Action<Command> _execute;

        public TestCommand(Action<Command> execute)
        {
            _execute = execute;
        }

        public override void Execute()
        {
            _execute(this);
        }
    }
}
