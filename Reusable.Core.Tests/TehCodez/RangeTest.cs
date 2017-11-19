using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Tester;

namespace Reusable.Tests
{
    [TestClass]
    public class RangeTest
    {
        [TestMethod]
        public void ctor_InvalidRange_Throws()
        {
            Assert.ThrowsException<ArgumentException>(() => Range.Create(3, 1));
        }

        [TestMethod]
        public void Equatable_IsCanononical_True()
        {
            Assert.That.Equatable().IsCanonical(Range.Create(1, 3));
        }

        [TestMethod]
        public void Equals_SameRanges_True()
        {
            Assert.That.Equatable().EqualsMethod().IsTrue(Range.Create(1, 3), Range.Create(1, 3));
            Assert.That.Equatable().EqualsMethod().IsTrue(Range.Create(1, 1), Range.Create(1, 1));
        }

        [TestMethod]
        public void Equals_DifferentRanges_True()
        {
            Assert.That.Equatable().EqualsMethod().IsFalse(Range.Create(1, 3), Range.Create(4, 8));
            Assert.That.Equatable().EqualsMethod().IsFalse(Range.Create(1, 1), Range.Create(2, 2));
        }

        [TestMethod]
        public void ContainsInclusive_InRange_True()
        {
            Assert.IsTrue(Range.Create(1, 3).ContainsInclusive(2));
            Assert.IsTrue(Range.Create(1, 3).ContainsInclusive(1));
            Assert.IsTrue(Range.Create(1, 3).ContainsInclusive(3));
        }

        [TestMethod]
        public void ContainsInclusive_NotInRange_False()
        {
            Assert.IsFalse(Range.Create(1, 3).ContainsInclusive(0));
            Assert.IsFalse(Range.Create(1, 3).ContainsInclusive(4));
        }

        [TestMethod]
        public void ContainsExclusive_InRange_True()
        {
            Assert.IsTrue(Range.Create(1, 3).ContainsExclusive(2));
        }

        [TestMethod]
        public void ContainsExclusive_NotInRange_False()
        {
            Assert.IsFalse(Range.Create(1, 3).ContainsExclusive(1));
            Assert.IsFalse(Range.Create(1, 3).ContainsExclusive(3));
        }

        [TestMethod]
        public void OverlapsInclusive_InRange_True()
        {
            Assert.IsTrue(Range.Create(1, 3).OverlapsInclusive(Range.Create(3, 5)));
            Assert.IsTrue(Range.Create(1, 3).OverlapsInclusive(Range.Create(2, 5)));
        }

        [TestMethod]
        public void OverlapsInclusive_NotInRange_False()
        {
            Assert.IsFalse(Range.Create(1, 3).OverlapsInclusive(Range.Create(4, 5)));
            Assert.IsFalse(Range.Create(1, 3).OverlapsInclusive(Range.Create(-4, 0)));
        }

        [TestMethod]
        public void OverlapsExclusive_InRange_True()
        {
            Assert.IsTrue(Range.Create(1, 3).OverlapsExclusive(Range.Create(2, 5)));
            Assert.IsTrue(Range.Create(1, 3).OverlapsExclusive(Range.Create(0, 2)));
        }

        [TestMethod]
        public void OverlapsExclusive_NotInRange_False()
        {
            Assert.IsFalse(Range.Create(1, 3).OverlapsExclusive(Range.Create(3, 5)));
            Assert.IsFalse(Range.Create(1, 3).OverlapsExclusive(Range.Create(0, 1)));
        }
    }
}
