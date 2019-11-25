using System.Linq;
using System.Linq.Custom;
using Xunit;

namespace Reusable.Commander
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
            Assert.Equal("foo", commandLine.Single(MultiName.Command));
        }

        [Fact]
        public void Can_parse_command_with_mixed_parameters()
        {
            var commandLine = Parser.Parse("foo qux -bar baz").ToList().Single();
            Assert.Equal("foo", commandLine.Single(MultiName.Command));
            Assert.Equal("qux", commandLine.Single(MultiName.Command).ElementAt(1));
            Assert.Equal("baz", commandLine.Single("bar"));
        }

        [Fact]
        public void Can_parse_multiple_commands()
        {
            var commandLines = Parser.Parse("foo.bar -baz -qux quux baar | bar.baz -foo").ToList();
            Assert.Equal(2, commandLines.Count);
            Assert.Equal("foo.bar", commandLines[0].Single(MultiName.Command));
            Assert.Equal("bar.baz", commandLines[1].Single(MultiName.Command));
        }
    }
}