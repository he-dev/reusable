using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.CommandLine.Collections;
using Reusable.CommandLine.Data;
using Reusable.TestBase.Collections;

namespace Reusable.CommandLine.Tests.Collections
{
    [TestClass]
    public class ArgumentLookupTest_
    {
        [TestMethod]
        public void ToCommandLine_VariousArguments1_Formatted()
        {
            var arguments = new ArgumentLookup
            {
                new CommandLineArgument(ImmutableNameSet.Create("arg1"))
                {
                    "true",
                },
                new CommandLineArgument(ImmutableNameSet.Create("arg2")),
                new CommandLineArgument(ImmutableNameSet.Create("arg3"))
                {
                    "1",
                    "2"
                }
            };

            Assert.AreEqual("-arg1:true -arg2 -arg3:1, 2", arguments.ToCommandLineString("-:"));
        }

        [TestMethod]
        public void ToCommandLine_VariousArguments2_Formatted()
        {
            var arguments = new ArgumentLookup
            {
                new CommandLineArgument(ImmutableNameSet.Create("arg1"))
                {
                    "true",
                },
                new CommandLineArgument(ImmutableNameSet.Create("arg2")),
                new CommandLineArgument(ImmutableNameSet.Create("arg3"))
                {
                    "1",
                    "2"
                }
            };

            Assert.AreEqual("/arg1 true /arg2 /arg3 1, 2", arguments.ToCommandLineString("/ "));
        }
    }

    [TestClass]
    public class ArgumentLookupTest : LookupTest<IImmutableNameSet, string>
    {
        protected override ILookup<IImmutableNameSet, string> GetEmptyLookup()
        {
            return new ArgumentLookup();
        }

        protected override ILookup<IImmutableNameSet, string> GetNonEmptyLookup()
        {
            return new ArgumentLookup
            {
                new CommandLineArgument(ImmutableNameSet.Create("arg1")) { "true" },
                new CommandLineArgument(ImmutableNameSet.Create("arg2")),
                new CommandLineArgument(ImmutableNameSet.Create("arg3")) { "1", "2" }
            };
        }

        protected override IEnumerable<(IImmutableNameSet Key, IEnumerable<string> Elements)> GetExistingKeys()
        {
            yield return (ImmutableNameSet.Create("arg1"), new[] { "true" });
            yield return (ImmutableNameSet.Create("arg2"), new string[0]);
            yield return (ImmutableNameSet.Create("arg3"), new[] { "1", "2" });
        }

        protected override IEnumerable<(IImmutableNameSet Key, IEnumerable<string> Elements)> GetNonExistingKeys()
        {
            yield return (ImmutableNameSet.Create("arg4"), Enumerable.Empty<string>());
            yield return (ImmutableNameSet.Create("arg5"), Enumerable.Empty<string>());
        }
    }
}
