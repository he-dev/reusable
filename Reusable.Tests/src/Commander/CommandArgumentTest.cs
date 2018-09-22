using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Commander;

namespace Reusable.Tests.Commander
{
    [TestClass]
    public class CommandArgumentTest
    {
        [TestMethod]
        public void ToString_WithKey_Formatted()
        {
            var arguments = new CommandArgument("foo")
            {
                "bar",
                "baz qux"
            };

            Assert.AreEqual("-foo=bar, \"baz qux\"", arguments.ToString());
        }

        [TestMethod]
        public void ToString_WithoutKey_Formatted()
        {
            var arguments = new CommandArgument(SoftString.Empty)
            {
                "bar",
                "baz qux"
            };

            Assert.AreEqual("bar, \"baz qux\"", arguments.ToString());
        }
    }
}