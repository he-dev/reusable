using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Commander;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests.Commander
{
    [TestClass]
    public class CommandLineParserTest
    {
        private static readonly ICommandLineParser Parser = new CommandLineParser(new CommandLineTokenizer());

        [TestMethod]
        public void Parse_Empty_EmptyCollection()
        {
            var commandLines = Parser.Parse(string.Empty).ToList();
            Assert.That.Collection().IsEmpty(commandLines);
        }

        [TestMethod]
        public void Parse_SingleCommand_SingleCommand()
        {
            var commandLine = Parser.Parse("foo").ToList().Single();
            Assert.AreEqual(Identifier.Create("foo"), commandLine.CommandId());
        }

        [TestMethod]
        public void Parse_CommandWithArguments_CommandWithArguments()
        {
            var commandLine = Parser.Parse("foo qux -bar baz").ToList().Single();
            Assert.AreEqual(Identifier.Create("foo"), commandLine.CommandId());
        }

        [TestMethod]
        public void Parse_MultipleCommands_MultipleResults()
        {
            var commandLines = Parser.Parse("foo.bar -baz -qux quux baar | bar.baz -foo").ToList();
            Assert.That.Collection().CountEquals(2, commandLines);
        }
    }
}