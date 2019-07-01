using System.Linq;
using System.Linq.Custom;
using Reusable.Commander;
using Xunit;

namespace Reusable.Tests.Commander
{
    public class CommandLineParserTest
    {
        private static readonly ICommandLineParser Parser = new CommandLineParser(new CommandLineTokenizer());

        [Fact]
        public void Can_parse_empty_command_line()
        {
            var commandLines = Parser.Parse(string.Empty).ToList();
            Assert.True(commandLines.Empty());
        }

        [Fact]
        public void Can_parse_single_command()
        {
            var commandLine = Parser.Parse("foo").ToList().Single();
            Assert.Equal("foo", commandLine[NameSet.Command]?.Single());
        }

        [Fact]
        public void Can_parse_command_with_mixed_parameters()
        {
            var commandLine = Parser.Parse("foo qux -bar baz").ToList().Single();
            Assert.Equal("foo", commandLine[NameSet.Command]?.Single());
            Assert.Equal("qux", commandLine[NameSet.FromPosition(1)]?.Single());
            Assert.Equal("baz", commandLine[NameSet.FromName("bar")]?.Single());
        }

        [Fact]
        public void Can_parse_multiple_commands()
        {
            var commandLines = Parser.Parse("foo.bar -baz -qux quux baar | bar.baz -foo").ToList();
            Assert.Equal(2, commandLines.Count);
            Assert.Equal("foo.bar", commandLines[0][NameSet.Command].Single());
            Assert.Equal("bar.baz", commandLines[1][NameSet.Command].Single());
        }
    }
}