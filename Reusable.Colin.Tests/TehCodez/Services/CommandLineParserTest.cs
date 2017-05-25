using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Colin.Collections;
using Reusable.Colin.Services;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.Colin.Tests.Services
{
    [TestClass]
    public class CommandLineParserTest
    {
        [TestMethod]
        public void Parse_Empty_EmptyCollection()
        {
            var arguments = new string[0].Parse('-').ToList();
            arguments.Verify().IsEmpty();
        }

        [TestMethod]
        public void Parse_SingleCommand_SingleCommand()
        {
            var arguments = new[] { "foo" }.Parse('-').ToList().First();
            arguments.CommandName().Verify().IsEqual(ImmutableNameSet.Create("foo"), ImmutableNameSet.Comparer);
        }

        [TestMethod]
        public void Parse_CommandWithArguments_CommandWithArguments()
        {
            var arguments = new[] { "foo", "qux", "-bar", "baz" }.Parse('-').ToList().First();
            arguments.CommandName().Verify().IsEqual(ImmutableNameSet.Create("foo"), ImmutableNameSet.Comparer);

            //arguments.Verify().SequenceEqual(new[] {});
        }

        [TestMethod]
        public void Parse_MultipleCommands_MultipleResults()
        {
            var arguments = new[] { "foo.bar", "-baz", "-qux", "quux baar", "|", "bar.baz", "-foo" }.Parse('-').ToList();
            arguments.Count.Verify().IsEqual(2);

        }
    }
}
