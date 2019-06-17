using System;
using System.Linq;
using Reusable.Commander;
using Xunit;

namespace Reusable.Tests.Commander
{
    public class CommandLineTest
    {
        [Fact]
        public void Add_ArgumentName_ArgumentName()
        {
            var commandLine = new CommandLine
            {
                "foo"
            };

            Assert.Equal(1, commandLine.Count);

            var commandArgument = commandLine.Single();

            Assert.Equal(Identifier.Create(("foo", NameOption.CommandLine)), commandArgument.Key);
            Assert.Equal(0, commandArgument.Count());
        }

        [Fact]
        public void Add_ArgumentWithValues_ArgumentWithValues()
        {
            var commandLine = new CommandLine
            {
                { "foo", "bar" },
                { "foo", "baz" },
            };

            Assert.Equal(1, commandLine.Count);

            var commandArgument = commandLine.Single();

            Assert.Equal(Identifier.Create(("foo", NameOption.CommandLine)), commandArgument.Key);
            Assert.Equal(2, commandArgument.Count());
            Assert.Equal("bar", commandArgument.ElementAt(0));
            Assert.Equal("baz", commandArgument.ElementAt(1));
        }
    }
}