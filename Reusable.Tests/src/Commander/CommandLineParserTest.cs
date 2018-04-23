using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Utilities.MSTest;
using SoftKeySet = Reusable.Collections.ImmutableKeySet<Reusable.SoftString>;

namespace Reusable.Commander.Tests
{
    [TestClass]
    public class CommandLineParserTest
    {
        private static readonly ICommandLineParser Parser = new CommandLineParser(new CommandLineTokenizer());

        [TestMethod]
        public void Parse_Empty_EmptyCollection()
        {
            var arguments = Parser.Parse("").ToList();
            Assert.That.Collection().IsEmpty(arguments);
        }

        [TestMethod]
        public void Parse_SingleCommand_SingleCommand()
        {
            var arguments = Parser.Parse("foo").ToList().First();
            Assert.AreEqual(SoftKeySet.Create("foo"), arguments.CommandName());
        }

        [TestMethod]
        public void Parse_CommandWithArguments_CommandWithArguments()
        {
            var arguments = Parser.Parse("foo qux -bar baz").ToList().First();
            Assert.AreEqual(SoftKeySet.Create("foo"), arguments.CommandName());

            //arguments.Verify().SequenceEqual(new[] {});
        }

        [TestMethod]
        public void Parse_MultipleCommands_MultipleResults()
        {
            var arguments = Parser.Parse("foo.bar -baz -qux quux baar | bar.baz -foo").ToList();
            Assert.That.Collection().CountEquals(2, arguments);

        }
    }
}
