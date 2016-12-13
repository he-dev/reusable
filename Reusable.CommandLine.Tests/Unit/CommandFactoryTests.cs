using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Candle.Tests.Unit
{
    [TestClass]
    public class CommandFactoryTests
    {
        [TestMethod]
        public void CreateCommand_CreatesCommandFromArguments()
        {
            var cmdFactory = new CommandFactory();
            var arguments = new ArgumentCollection();
            arguments.Add(Argument.Anonymous, "foo");
            var cmd = cmdFactory.CreateCommand(typeof(FooCommand), arguments);
            cmd.Verify().IsNotNull();
            cmd.Verify().IsInstanceOfType(typeof(FooCommand));
        }

        private class FooCommand : Command
        {
            public override int Execute()
            {
                throw new NotImplementedException();
            }
        }
    }
}
