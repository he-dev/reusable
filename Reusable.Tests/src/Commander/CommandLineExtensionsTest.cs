using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Collections;
using SoftKeySet = Reusable.Collections.ImmutableKeySet<Reusable.SoftString>;

namespace Reusable.Commander.Tests
{
    [TestClass]
    public class CommandLineExtensionsTest
    {
        //[TestMethod]
        //public void Anonymous_DoesNotContain_Empty()
        //{
        //    var arguments = new ArgumentLookup
        //    {
        //        { "foo", "bar" }
        //    };
        //    Assert.IsFalse(arguments.Anonymous().Any());
        //}

        //[TestMethod]
        //public void Anonymous_Contains_NonEmpty()
        //{
        //    var arguments = new ArgumentLookup
        //    {
        //        { ImmutableNameSet.Empty, "baz" },
        //        { ImmutableNameSet.Empty, "qux" },
        //        { ImmutableNameSet.Create("foo"), "bar" },
        //    };
        //    Assert.IsTrue(arguments.Anonymous().Any());
        //    Assert.AreEqual(2, arguments.Anonymous().Count());
        //}

        //[TestMethod]
        //public void CommandName_DoesNotContains_Null()
        //{
        //    var arguments = new ArgumentLookup
        //    {
        //        { "foo", "bar" },
        //    };
        //    Assert.IsNull(arguments.CommandName());
        //}

        [TestMethod]
        public void CommandName_Contains_String()
        {
            var arguments = new CommandLine
            {
                { SoftString.Empty, "baz" },
                { SoftString.Empty, "qux" },
                { SoftString.Create("foo"), "bar" },
            };
            Assert.AreEqual(SoftKeySet.Create("baz"), arguments.CommandName());
        }
    }
}
