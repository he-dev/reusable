using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Shelly.Collections;
using Reusable.Fuse.Testing;
using Reusable.Fuse;

namespace Reusable.Shelly.Tests.Collections
{
    [TestClass]
    public class ArgumentCollectionTest
    {
        [TestMethod]
        public void this_Name_Position_NamedArguments_NamedArgument()
        {
            var arguments = new ArgumentCollection
            {
                { "foo", "bar" },
                { "foo", "baz" },
                { "qux", null }
            };

            arguments["foo"].Verify().IsNotNull().IsTrue(x => x.Count == 2);
            arguments["qux"].Verify().IsNotNull().IsTrue(x => x.Count == 0);
        }

        [TestMethod]
        public void this_Name_Position_UnNamedArguments_UnNamedArgument()
        {
            var arguments = new ArgumentCollection
            {
                { "", "foo" },
                { "", "bar" },
                { "", "baz" },
                { "qux", null }
            };

            arguments["foo"].Verify().IsNull();
            arguments[0].Verify().IsNull();
            arguments[1].Verify().IsNotNull().IsTrue(x => x[0] == "bar");
            arguments["qux"].Verify().IsNotNull().IsTrue(x => x.Count == 0);
        }
    }
}
