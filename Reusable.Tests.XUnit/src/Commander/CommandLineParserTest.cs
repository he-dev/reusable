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
        public void Parse_Empty_EmptyCollection()
        {
            var commandLines = Parser.Parse(string.Empty).ToList();
            Assert.True(commandLines.Empty());
        }

        [Fact]
        public void Parse_SingleCommand_SingleCommand()
        {
            var commandLine = Parser.Parse("foo").ToList().Single();
            Assert.Equal(Identifier.Create("foo"), commandLine.CommandId());
        }

        [Fact]
        public void Parse_CommandWithArguments_CommandWithArguments()
        {
            var commandLine = Parser.Parse("foo qux -bar baz").ToList().Single();
            Assert.Equal(Identifier.Create("foo"), commandLine.CommandId());
        }

        [Fact]
        public void Parse_MultipleCommands_MultipleResults()
        {
            var commandLines = Parser.Parse("foo.bar -baz -qux quux baar | bar.baz -foo").ToList();
            Assert.Equal(2, commandLines.Count);
        }
    }
}