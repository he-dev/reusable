using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftKeySet = Reusable.Collections.ImmutableKeySet<Reusable.SoftString>;

namespace Reusable.Commander.Tests
{
    [TestClass]
    public class CommandArgumentTest
    {
        [TestMethod]
        public void ToString_WithKey_Formatted()
        {
            var arguments = new CommandArgument(SoftKeySet.Create("foo"))
            {
                "bar",
                "baz qux"
            };

            Assert.AreEqual("-foo:bar, \"baz qux\"", arguments.ToString());
        }

        [TestMethod]
        public void ToString_WithoutKey_Formatted()
        {
            var arguments = new CommandArgument(SoftKeySet.Empty)
            {
                "bar",
                "baz qux"
            };

            Assert.AreEqual("bar, \"baz qux\"", arguments.ToString());
        }
    }
}