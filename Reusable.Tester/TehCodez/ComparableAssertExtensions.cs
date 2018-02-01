using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Extensions;

namespace Reusable.Utilities.MSTest
{
    public interface IComparableAssert { }
    public interface IComparableCompareToAssert { }

    public static class ComparableAssertExtensions
    {
        public static IComparableCompareToAssert CompareTo(this IComparableAssert assert) => default(IComparableCompareToAssert);

        public static void IsCanonical<T>(this IComparableAssert assert, IComparable<T> comparable, T less, T equal, T greater)
        {
            Assert.IsTrue(comparable.CompareTo(less) > 0, CreateMessage("x.CompareTo(less) > 0"));
            Assert.IsTrue(comparable.CompareTo(equal) == 0, CreateMessage("x.CompareTo(equal) == 0"));
            Assert.IsTrue(comparable.CompareTo(greater) < 0, CreateMessage("x.CompareTo(greater) < 0"));

            string CreateMessage(string requirement)
            {
                return $"{typeof(IComparable<T>).ToPrettyString()} violates the {requirement.QuoteWith("'")} requirement.";
            }
        }
    }

    public static class ComparableCompareToAssertExtensions
    {
        public static void IsGreaterThen<T>(this IComparableCompareToAssert assert, IComparable<T> comparable, params T[] others)
        {
            Check(comparable, others, result => result > 0);
        }

        public static void IsEqualTo<T>(this IComparableCompareToAssert assert, IComparable<T> comparable, params T[] others)
        {
            Check(comparable, others, result => result == 0);
        }

        public static void IsLessThen<T>(this IComparableCompareToAssert assert, IComparable<T> comparable, params T[] others)
        {
            Check(comparable, others, result => result < 0);
        }

        private static void Check<T>(IComparable<T> comparable, IEnumerable<T> others, Func<int, bool> condition)
        {
            var i = 0;
            foreach (var other in others)
            {
                var result = comparable.CompareTo(other);
                Assert.IsTrue(
                    condition(result),
                    $"{nameof(IComparable<T>.CompareTo)} was {result.Stringify()} and failed for {comparable.Stringify()} and {other.Stringify()} at [{i++}]."
                );
            }
        }
    }
}
