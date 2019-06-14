using System.Linq;
using Reusable.Commander;
using Reusable.Exceptionize;
using Xunit;

namespace Reusable.Tests.Commander
{
    public class CommandLineExtensionsTest
    {
        [Fact]
        public void AnonymousValues_None_Empty()
        {
            var commandLine = new CommandLine
            {
                { "foo", "bar" }
            };
            Assert.False(commandLine.AnonymousValues().Any());
        }

        [Fact]
        public void AnonymousValues_Some_NonEmpty()
        {
            var commandLine = new CommandLine
            {
                { Identifier.Empty, "baz" },
                { Identifier.Empty, "qux" },
                { Identifier.Empty, "bar" },
            };
            Assert.Equal(3, commandLine.AnonymousValues().Count());
        }

        [Fact]
        public void CommandId_DoesNotContain_Throws()
        {
            var commandLine = new CommandLine
            {
                { "foo", "bar" },
            };
            Assert.ThrowsAny<DynamicException>(
                () => commandLine.CommandId()
                //filter => filter.When(name: "^CommandNameNotFound")
            );
        }

        [Fact]
        public void CommandId_Contains_Identifier()
        {
            var commandLine = new CommandLine
            {
                { Identifier.Empty, "baz" },
                { Identifier.Empty, "qux" },
                { Identifier.Create("foo"), "bar" },
            };
            Assert.Equal(Identifier.Create("baz"), commandLine.CommandId());
        }
    }
}