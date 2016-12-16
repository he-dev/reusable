using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.Shelly.Tests
{
    [TestClass]
    public class CommandLineParserTest
    {
        [TestMethod]
        public void Parse_Empty()
        {
            var result = new CommandLineParser().Parse(new string[0], "-", ":", new string[0]);
            result.CommandName.Verify().IsNullOrEmpty();
            result.Arguments.Verify().IsEmpty();
        }

        [TestMethod]
        public void Parse_Command()
        {
            var result = new CommandLineParser().Parse(new[] { "foo" }, "-", ":", new[] { "foo" });
            result.CommandName.Verify().IsEqual("foo");
            result.Arguments.Verify().IsEmpty();
        }

        [TestMethod]
        public void Parse_CommandWithArguments()
        {
            var result = new CommandLineParser().Parse(new[] { "foo", "qux", "-bar:baz" }, "-", ":", new[] { "foo" });
            result.CommandName.Verify().IsEqual("foo");
            result.Arguments.Count().Verify().IsEqual(2);
            result.Arguments.Single(x => x.Key == string.Empty).First().Verify().IsEqual("qux");
            result.Arguments.Single(x => x.Key == "bar").First().Verify().IsEqual("baz");
        }
    }
}
