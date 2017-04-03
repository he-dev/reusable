using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Colin.Collections;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.Colin.Tests
{
    [TestClass]
    public class CommandLineParserTest
    {
        [TestMethod]
        public void Parse_Empty_EmptyCollection()
        {
            var arguments = CommandLineParser.Parse(new string[0], "-").ToList();
            arguments.Verify().IsEmpty();
        }

        [TestMethod]
        public void Parse_SingleCommand_SingleCommand()
        {
            var arguments = CommandLineParser.Parse(new[] { "foo" }, "-").ToList().First();
            arguments.CommandName.Verify().IsEqual(ImmutableNameSet.Create("foo"), ImmutableNameSet.Comparer);
        }

        [TestMethod]
        public void Parse_CommandWithArguments_CommandWithArguments()
        {
            var arguments = CommandLineParser.Parse(new[] { "foo", "qux", "-bar", "baz" }, "-").ToList().First();
            arguments.CommandName.Verify().IsEqual(ImmutableNameSet.Create("foo"), ImmutableNameSet.Comparer);

            //arguments.Verify().SequenceEqual(new[] {});
        }

        [TestMethod]
        public void Parse_MultipleCommands_MultipleResults()
        {
            var arguments = CommandLineParser.Parse(new[] { "foo.bar", "-baz", "-qux", "quux baar", "|", "bar.baz", "-foo" }, "-").ToList();
            arguments.Count.Verify().IsEqual(2);

        }
    }
}
