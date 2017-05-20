using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Colin.Collections;

namespace Reusable.Colin.Tests.Collections
{
    [TestClass]
    public class ImmutableNameSetEqualityComparerTest
    {
        [TestMethod]
        public void Equals_DifferentNames_False()
        {
            Assert.IsFalse(
                ImmutableNameSet
                    .Comparer
                    .Equals(
                        ImmutableNameSet.Create("foo"), 
                        ImmutableNameSet.Create("bar")));
        }

        [TestMethod]
        public void Equals_SameNames_True()
        {
            Assert.IsTrue(
                ImmutableNameSet
                    .Comparer
                    .Equals(
                        ImmutableNameSet.Create("foo"),
                        ImmutableNameSet.Create("foo")));
        }

        [TestMethod]
        public void Equals_CommonNames_True()
        {
            Assert.IsTrue(
                ImmutableNameSet
                    .Comparer
                    .Equals(
                        ImmutableNameSet.Create("foo"),
                        ImmutableNameSet.Create("bar", "foo")));
        }
    }
}
