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
            builder.Register<Test1Command>().AsDefault();
            var cmdLine = builder.Build();

            cmdLine.Execute(Enumerable.Empty<string>());
        }

        [TestMethod]
        public void Execute_Throw_NoDefaultCommand()
        {
            var builder = new CommandLineBuilder();
            builder.Register<Test1Command>();
            var cmdLine = builder.Build();

            cmdLine.Execute(Enumerable.Empty<string>());
        }

        private class Test1Command : Command
        {
            public override void Execute()
            {
                
            }
        }
    }
}
