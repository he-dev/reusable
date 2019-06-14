using System;
using Xunit;
using Reusable.Utilities.Xunit.Extensions;

// ReSharper disable once CheckNamespace
namespace Reusable.Tests
{
    public class RangeTest
    {
        [Fact]
        public void ctor_throws_when_min_less_than_max()
        {
            Assert.ThrowsAny<ArgumentException>(() => Range.Create(3, 1));
        }

        [Fact]
        public void Implements_canonical_IEquatable()
        {
            Assert.That.Equatable().IsCanonical(Range.Create(1, 3));
        }

        [Fact]
        public void Equals_SameRanges_True()
        {
            //Assert.That.Equatable().EqualsMethod().IsTrue(Range.Create(1, 3), Range.Create(1, 3));
            //Assert.That.Equatable().EqualsMethod().IsTrue(Range.Create(1, 1), Range.Create(1, 1));
            Assert.Equal(Range.Create(1, 3), Range.Create(1, 3));
            Assert.Equal(Range.Create(1, 1), Range.Create(1, 1));
        }

        [Fact]
        public void Equals_DifferentRanges_True()
        {
//            Assert.That.Equatable().EqualsMethod().IsFalse(Range.Create(1, 3), Range.Create(4, 8));
//            Assert.That.Equatable().EqualsMethod().IsFalse(Range.Create(1, 1), Range.Create(2, 2));
            Assert.NotEqual(Range.Create(1, 3), Range.Create(4, 8));
            Assert.NotEqual(Range.Create(1, 1), Range.Create(2, 2));
        }

        [Fact]
        public void ContainsInclusive_InRange_True()
        {
            Assert.True(Range.Create(1, 3).ContainsInclusive(2));
            Assert.True(Range.Create(1, 3).ContainsInclusive(1));
            Assert.True(Range.Create(1, 3).ContainsInclusive(3));
        }

        [Fact]
        public void ContainsInclusive_NotInRange_False()
        {
            Assert.False(Range.Create(1, 3).ContainsInclusive(0));
            Assert.False(Range.Create(1, 3).ContainsInclusive(4));
        }

        [Fact]
        public void ContainsExclusive_InRange_True()
        {
            Assert.True(Range.Create(1, 3).ContainsExclusive(2));
        }

        [Fact]
        public void ContainsExclusive_NotInRange_False()
        {
            Assert.False(Range.Create(1, 3).ContainsExclusive(1));
            Assert.False(Range.Create(1, 3).ContainsExclusive(3));
        }

        [Fact]
        public void OverlapsInclusive_InRange_True()
        {
            Assert.True(Range.Create(1, 3).OverlapsInclusive(Range.Create(3, 5)));
            Assert.True(Range.Create(1, 3).OverlapsInclusive(Range.Create(2, 5)));
        }

        [Fact]
        public void OverlapsInclusive_NotInRange_False()
        {
            Assert.False(Range.Create(1, 3).OverlapsInclusive(Range.Create(4, 5)));
            Assert.False(Range.Create(1, 3).OverlapsInclusive(Range.Create(-4, 0)));
        }

        [Fact]
        public void OverlapsExclusive_InRange_True()
        {
            Assert.True(Range.Create(1, 3).OverlapsExclusive(Range.Create(2, 5)));
            Assert.True(Range.Create(1, 3).OverlapsExclusive(Range.Create(0, 2)));
        }

        [Fact]
        public void OverlapsExclusive_NotInRange_False()
        {
            Assert.False(Range.Create(1, 3).OverlapsExclusive(Range.Create(3, 5)));
            Assert.False(Range.Create(1, 3).OverlapsExclusive(Range.Create(0, 1)));
        }

        [Fact]
        public void Can_create_from_single_item()
        {
            Assert.Equal(Range.Create(1), new[] { 1 }.ToRange());
        }

        [Fact]
        public void Can_create_from_two_items()
        {
            Assert.Equal(Range.Create(1, 3), new[] { 1, 3 }.ToRange());
        }
    }
}