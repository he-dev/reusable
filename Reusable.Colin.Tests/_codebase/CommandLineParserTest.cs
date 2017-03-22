using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using Reusable.Shelly.Collections;

namespace Reusable.Shelly.Tests
{
    [TestClass]
    public class CommandLineParserTest
    {
        [TestMethod]
        public void Parse_Empty_EmptyCollection()
        {
            var arguments = CommandLineParser.Parse(new string[0], "-");
            arguments.Verify().IsNotNull();
            arguments.Verify().IsEmpty();
        }

        [TestMethod]
        public void Parse_Command()
        {
            var arguments = CommandLineParser.Parse(new[] { "foo" }, "-");
            arguments.CommandName.Verify().IsTrue(x => x.Overlaps(ImmutableNameSet.Create("foo")));
        }

        [TestMethod]
        public void Parse_CommandWithArguments()
        {
            var arguments = CommandLineParser.Parse(new[] { "foo", "qux", "-bar:baz" }, "-");
            arguments.CommandName.Verify().IsTrue(x => x.Overlaps(ImmutableNameSet.Create("foo")));

            arguments.Count().Verify().IsEqual(2);
            //arguments.Single(x => x.Key == string.Empty).First().Verify().IsEqual("qux");
            //arguments.Single(x => x.Key == "bar").First().Verify().IsEqual("baz");
        }
    }
}
