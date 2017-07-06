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
            var arguments = new ArgumentGrouping(ImmutableNameSet.Create("foo"))
            {
                "bar",
                "baz qux"
            };

            Assert.AreEqual("-foo:bar, \"baz qux\"", arguments.ToCommandLineString("-:"));
        }

        [TestMethod]
        public void ToCommandLine_WithoutKey_Formatted()
        {
            var arguments = new ArgumentGrouping(ImmutableNameSet.Empty)
            {
                "bar",
                "baz qux"
            };

            Assert.AreEqual("bar, \"baz qux\"", arguments.ToCommandLineString("-:"));
        }
    }
}
