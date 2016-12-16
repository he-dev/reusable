using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.Shelly.Tests
{
    [TestClass]
    public class CommandFactoryTest
    {
        [TestMethod]
        public void CreateCommand_WithoutParameters_WithoutArguments()
        {
            var builder = new CommandLineBuilder();
            builder.Register<Test1Command>();
            var cmdLine = builder.Build();

            var cmdFactory = new CommandFactory();
            var cmd = cmdFactory.CreateCommand(cmdLine.Commands.First(), new IGrouping<string, string>[0], cmdLine);
            cmd.Verify().IsNotNull();
        }

        private class Test1Command : Command
        {
            public override void Execute()
            {
                throw new NotImplementedException();
            }
        }
    }
}
