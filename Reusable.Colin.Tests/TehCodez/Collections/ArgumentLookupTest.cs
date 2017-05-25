using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Colin.Collections;
using Reusable.Colin.Data;

namespace Reusable.Colin.Tests.Collections
{
    [TestClass]
    public class ArgumentLookupTest
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

            Assert.AreEqual("-arg1:true -arg2 -arg3:1, 2", arguments.ToCommandLine("-:"));
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

            Assert.AreEqual("/arg1 true /arg2 /arg3 1, 2", arguments.ToCommandLine("/ "));
        }
    }
}
