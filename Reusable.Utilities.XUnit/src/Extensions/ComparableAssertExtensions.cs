using System;
using System.Collections.Generic;
using Reusable.Extensions;
using Xunit;

namespace Reusable.Utilities.Xunit.Extensions
{
    public interface IComparerAssert { }

    public interface IComparableAssert { }

    public interface IComparableCompareToAssert { }

    public static class ComparerAssertExtensions
    {
        public static void IsCanonical<T>(this IComparerAssert assert, T value, T less, T equal, T greater, IComparer<T> comparer)
        {
            var typeName = typeof(IComparer<T>).ToPrettyString();

            Assert.True(comparer.Compare(value, value) == 0, $"{typeName} violates the expression: value == value.");
            Assert.True(comparer.Compare(value, equal) == 0, $"{typeName} violates the expression: value == equal.");
            Assert.True(comparer.Compare(less, value) < 0, $"{typeName} violates the expression: less < value.");
            Assert.True(comparer.Compare(greater, value) > 0, $"{typeName} violates the expression: greater > value.");
        }
    }

    public static class ComparableAssertExtensions
    {
        public static IComparableCompareToAssert CompareTo(this IComparableAssert assert) => default;

        public static void IsCanonical<T>(this IComparableAssert assert, IComparable<T> comparable, T less, T equal, T greater)
        {
            Assert.True(comparable.CompareTo(less) > 0, CreateMessage("x.CompareTo(less) > 0"));
            Assert.True(comparable.CompareTo(equal) == 0, CreateMessage("x.CompareTo(equal) == 0"));
            Assert.True(comparable.CompareTo(greater) < 0, CreateMessage("x.CompareTo(greater) < 0"));

            string CreateMessage(string requirement)
            {
                return $"{typeof(IComparable<T>).ToPrettyString()} violates the {requirement.QuoteWith("'")} requirement.";
            }
        }
    }

    public static class ComparableCompareToAssertExtensions
    {
        public static void GreaterThan<T>(this IComparableCompareToAssert assert, IComparable<T> comparable, params T[] others)
        {
            Check(comparable, others, result => result > 0);
        }
        
        public static void GreaterThanOrEqual<T>(this IComparableCompareToAssert assert, IComparable<T> comparable, params T[] others)
        {
            Check(comparable, others, result => result >= 0);
        }

        public static void Equal<T>(this IComparableCompareToAssert assert, IComparable<T> comparable, params T[] others)
        {
            Check(comparable, others, result => result == 0);
        }

        public static void LessThan<T>(this IComparableCompareToAssert assert, IComparable<T> comparable, params T[] others)
        {
            Check(comparable, others, result => result < 0);
        }
        
        public static void LessThanOrEqual<T>(this IComparableCompareToAssert assert, IComparable<T> comparable, params T[] others)
        {
            Check(comparable, others, result => result <= 0);
        }

        private static void Check<T>(IComparable<T> comparable, IEnumerable<T> others, Func<int, bool> predicate)
        {
            var i = 0;
            foreach (var other in others)
            {
                var result = comparable.CompareTo(other);
                Assert.True(
                    predicate(result),
                    $"{nameof(IComparable<T>.CompareTo)} was {result.Stringify()} and failed for {comparable.Stringify()} and {other.Stringify()} at [{i++}]."
                );
            }
        }
    }
}