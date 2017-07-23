using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.CommandLine.Collections;
using Reusable.CommandLine.Tests.Commnads;

namespace Reusable.CommandLine.Tests
{
    [TestClass]
    public class CommandContainerExtensionsTest
    {
        [TestMethod]
        public void Find_DoesNotExist_Null()
        {
            var commands = CommandContainer.Empty;
            Assert.IsNull(commands.Find(ImmutableNameSet.Create("foo")));
        }

        [TestMethod]
        public void Find_SingleCommand_SingleCommand()
        {
            var foo = new FooCommand();
            var commands = CommandContainer.Empty.Add(foo);
            var result = commands.Find(ImmutableNameSet.Create("foo"));
            Assert.IsNotNull(result);
            Assert.AreEqual(ImmutableNameSet.Create("foo"), result.Name);
            //Assert.AreSame(foo, result);
        }

        [TestMethod]
        public void Find_MultipleCommands_SingleCommand()
        {
            var foo = new FooCommand();
            var commands = CommandContainer.Empty.Add(foo).Add<BarCommand>();
            var result = commands.Find(ImmutableNameSet.Create("foo"));
            Assert.IsNotNull(result);
            Assert.AreEqual(ImmutableNameSet.Create("foo"), result.Name);
            //Assert.AreSame(foo, result);
        }

        private class FooCommand : Commnads.FooCommand { }

        private class BarCommand : Commnads.FooCommand { }
    }
}
