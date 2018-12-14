using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Commander;
using Reusable.Exceptionizer;
using Reusable.Reflection;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests.Commander
{
    [TestClass]
    public class CommandLineExtensionsTest
    {
        [TestMethod]
        public void AnonymousValues_None_Empty()
        {
            var commandLine = new CommandLine
            {
                {"foo", "bar"}
            };
            Assert.IsFalse(commandLine.AnonymousValues().Any());
        }

        [TestMethod]
        public void AnonymousValues_Some_NonEmpty()
        {
            var commandLine = new CommandLine
            {
                {Identifier.Empty, "baz"},
                {Identifier.Empty, "qux"},
                {Identifier.Empty, "bar"},
            };
            Assert.AreEqual(3, commandLine.AnonymousValues().Count());
        }

        [TestMethod]
        public void CommandId_DoesNotContain_Throws()
        {
            var commandLine = new CommandLine
            {
                {"foo", "bar"},
            };
            Assert.That.Throws<DynamicException>(
                () => commandLine.CommandId(),
                filter => filter.When(name: "^CommandNameNotFound")
            );
        }

        [TestMethod]
        public void CommandId_Contains_Identifier()
        {
            var commandLine = new CommandLine
            {
                {Identifier.Empty, "baz"},
                {Identifier.Empty, "qux"},
                {Identifier.Create("foo"), "bar"},
            };
            Assert.AreEqual(Identifier.Create("baz"), commandLine.CommandId());
        }
    }
}