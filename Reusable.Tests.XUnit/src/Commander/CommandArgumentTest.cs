using Reusable.Commander;
using Xunit;

namespace Reusable.Tests.Commander
{
    public class CommandArgumentTest
    {
        [Fact]
        public void ToString_WithKey_Formatted()
        {
            var arguments = new CommandArgument("foo")
            {
                "bar",
                "baz qux"
            };

            Assert.Equal(@"-foo:bar, ""baz qux""", arguments.ToString());
        }

        [Fact]
        public void ToString_WithoutKey_Formatted()
        {
            var arguments = new CommandArgument()
            {
                "bar",
                "baz qux"
            };

            Assert.Equal(@"bar, ""baz qux""", arguments.ToString());
        }
    }
}