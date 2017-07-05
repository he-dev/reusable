using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.CommandLine.Collections;

namespace Reusable.CommandLine.Tests.Collections
{
    [TestClass]
    public class ArgumentLookupExtensionsTest
    {
        [TestMethod]
        public void Anonymous_DoesNotContain_Empty()
        {
            var arguments = new ArgumentLookup
            {
                { "foo", "bar" }
            };
            Assert.IsFalse(arguments.Anonymous().Any());
        }

        [TestMethod]
        public void Anonymous_Contains_NonEmpty()
        {
            var arguments = new ArgumentLookup
            {
                { ImmutableNameSet.Empty, "baz" },
                { ImmutableNameSet.Empty, "qux" },
                { ImmutableNameSet.Create("foo"), "bar" },
            };
            Assert.IsTrue(arguments.Anonymous().Any());
            Assert.AreEqual(2, arguments.Anonymous().Count());
        }

        [TestMethod]
        public void CommandName_DoesNotContains_Null()
        {
            var arguments = new ArgumentLookup
            {
                { "foo", "bar" },
            };
            Assert.IsNull(arguments.CommandName());
        }

        [TestMethod]
        public void CommandName_Contains_String()
        {
            var arguments = new ArgumentLookup
            {
                { ImmutableNameSet.Empty, "baz" },
                { ImmutableNameSet.Empty, "qux" },
                { ImmutableNameSet.Create("foo"), "bar" },
            };
            Assert.AreEqual(ImmutableNameSet.Create("baz"), arguments.CommandName());
        }
    }
}
