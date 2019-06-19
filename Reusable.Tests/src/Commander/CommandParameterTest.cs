using Reusable.Commander;
using Xunit;

namespace Reusable.Tests.Commander
{
    public class CommandParameterTest
    {
        [Fact]
        public void ToString_can_format_parameter_string()
        {
            var parameter = new CommandArgument(Identifier.FromName("foo"))
            {
                "bar",
                "baz qux"
            };

            Assert.Equal(@"-foo:bar, ""baz qux""", parameter.ToString());
        }

//        [Fact]
//        public void ToString_WithoutKey_Formatted()
//        {
//            var parameter = new CommandParameter(Identifier.FromName(""))
//            {
//                "bar",
//                "baz qux"
//            };
//
//            Assert.Equal(@"bar, ""baz qux""", parameter.ToString());
//        }
    }
}