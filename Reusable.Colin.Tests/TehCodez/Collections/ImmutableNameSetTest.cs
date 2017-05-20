using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Colin.Annotations;
using Reusable.Colin.Collections;

namespace Reusable.Colin.Tests.Collections
{
    [TestClass]
    public class ImmutableNameSetTest
    {
        [TestMethod]
        public void FromPropertyInfo_WithDefaultName_NameAndShortName()
        {
            var name = ImmutableNameSet.From(typeof(Foo).GetProperty(nameof(Foo.Bar)));
            Assert.AreEqual(ImmutableNameSet.Create("Bar"), name);
            Assert.AreEqual(ImmutableNameSet.Create("B"), name);
        }

        [TestMethod]
        public void FromPropertyInfo_WithCustomNames_CustomNames()
        {
            var name = ImmutableNameSet.From(typeof(Foo).GetProperty(nameof(Foo.Baz)));
            Assert.AreEqual(ImmutableNameSet.Create("Qux"), name);
            Assert.AreEqual(ImmutableNameSet.Create("Q"), name);
        }

        private class Foo
        {
            [Parameter]
            public string Bar { get; set; }

            [Parameter("Qux", "Q")]
            public string Baz { get; set; }
        }
    }
}
