using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.CommandLine.Annotations;
using Reusable.CommandLine.Collections;

namespace Reusable.CommandLine.Tests.Collections
{
    [TestClass]
    public class ImmutableNameSetTest
    {
        [TestMethod]
        public void Equals_DifferentNames_False()
        {
            Assert.AreNotEqual(ImmutableNameSet.Create("foo"), ImmutableNameSet.Create("bar"));
        }

        [TestMethod]
        public void Equals_SameNames_True()
        {
            Assert.AreEqual(ImmutableNameSet.Create("foo"), ImmutableNameSet.Create("foo"));
        }

        [TestMethod]
        public void Equals_CommonNames_True()
        {
            Assert.AreEqual(ImmutableNameSet.Create("foo"), ImmutableNameSet.Create("bar", "FOO"));
        }

        [TestMethod]
        public void From_Command_NoAttribute_DefaultName()
        {

        }

        [TestMethod]
        public void From_PropertyInfo_DefaultParameter_DefaultName()
        {
            var name = ImmutableNameSet.From(typeof(Foo).GetProperty(nameof(Foo.Bar)));
            Assert.AreEqual(ImmutableNameSet.Create("Bar"), name);
        }

        [TestMethod]
        public void From_PropertyInfo_AllowShortName_DefaultAndShortNames()
        {
            var name = ImmutableNameSet.From(typeof(Foo).GetProperty(nameof(Foo.Corge)));
            Assert.AreEqual(ImmutableNameSet.Create("Corge"), name);
            Assert.AreEqual(ImmutableNameSet.Create("C"), name);
        }

        [TestMethod]
        public void From_PropertyInfo_CustomNames_CustomNames()
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

            [Parameter(AllowShortName = true)]
            public string Corge { get; set; }
        }
    }
}
