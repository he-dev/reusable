using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.CommandLine.Collections;
using Reusable.CommandLine.Data;

namespace Reusable.CommandLine.Tests.Data
{
    [TestClass]
    public class CommandLineArgumentTest
    {
        [TestMethod]
        public void ToCommandLine_WithKey_Formatted()
        {
            var arguments = new CommandLineArgument(ImmutableNameSet.Create("foo"))
            {
                "bar",
                "baz qux"
            };

            Assert.AreEqual("-foo:bar, \"baz qux\"", arguments.ToCommandLine("-:"));
        }

        [TestMethod]
        public void ToCommandLine_WithoutKey_Formatted()
        {
            var arguments = new CommandLineArgument(ImmutableNameSet.Empty)
            {
                "bar",
                "baz qux"
            };

            Assert.AreEqual("bar, \"baz qux\"", arguments.ToCommandLine("-:"));
        }
    }
}
