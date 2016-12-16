using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using Reusable.Shelly.Data;

namespace Reusable.Shelly.Tests
{
    [TestClass]
    public class CommandLineBuilderTest
    {
        [TestMethod]
        public void ctor_CreatesDefaultBuilder()
        {
            var builder = new CommandLineBuilder();
            var cmdln = builder.Build();
            cmdln.ArgumentPrefix.Verify().IsEqual("-");
            cmdln.ArgumentValueSeparator.Verify().IsEqual(":");
            cmdln.Commands.Verify().IsEmpty();
        }

        [TestMethod]
        public void ctor_UseCustomArgumentSettings()
        {
            var builder = new CommandLineBuilder();
            builder.ArgumentPrefix("/").ArgumentValueSeparator("=");
            var cmdln = builder.Build();

            cmdln.ArgumentPrefix.Verify().IsEqual("/");
            cmdln.ArgumentValueSeparator.Verify().IsEqual("=");
            cmdln.Commands.Verify().IsEmpty();
        }

        [TestMethod]
        public void Register_CommandWithoutArguments()
        {
            var builder = new CommandLineBuilder();
            builder.Register<TestCommand>();
            var cmdln = builder.Build();

            cmdln.Commands.Count().Verify().IsEqual(1);
            var cmd = cmdln.Commands.Single();
            cmd.CommandType.Verify().IsTrue(x => x == typeof(TestCommand));
            cmd.Args.Verify(nameof(CommandInfo.Args)).IsNotNull().IsEmpty();
            cmd.IsDefault.Verify().IsFalse();
        }

        [TestMethod]
        public void Register_CommandWithArguments()
        {
            var builder = new CommandLineBuilder();
            builder.Register<TestCommand>(new object());
            var commandLine = builder.Build();

            commandLine.Commands.Count().Verify().IsEqual(1);
            var cmd = commandLine.Commands.Single();
            cmd.CommandType.Verify().IsTrue(x => x == typeof(TestCommand));
            cmd.Args.Verify(nameof(CommandInfo.Args)).IsNotNull().IsNotEmpty();
            cmd.Args.Single().Verify().IsInstanceOfType(typeof(object));
            cmd.IsDefault.Verify().IsFalse();
        }

        [TestMethod]
        public void AsDefault_FirstCommand()
        {
            var builder = new CommandLineBuilder();
            builder.Register<TestCommand>().AsDefault();
            var commandLine = builder.Build();

            commandLine.Commands.Count().Verify().IsEqual(1);
            var cmd = commandLine.Commands.Single();
            cmd.IsDefault.Verify(nameof(CommandInfo.IsDefault)).IsTrue();
        }

        [TestMethod]
        public void AsDefault_SecondCommandThrows()
        {
            new Action(() =>
            {
                var builder = new CommandLineBuilder();
                builder.Register<TestCommand>().AsDefault();
                builder.Register<Test2Command>().AsDefault();
            })
            .Verify().Throws<InvalidOperationException>();
        }

        private class TestCommand : Command
        {
            public TestCommand() { }

            public TestCommand(object arg) { }

            public override void Execute()
            {
                throw new NotImplementedException();
            }
        }

        private class Test2Command : Command
        {
            public override void Execute()
            {
                throw new NotImplementedException();
            }
        }
    }
}
