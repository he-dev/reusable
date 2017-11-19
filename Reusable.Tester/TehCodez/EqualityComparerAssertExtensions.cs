using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Extensions;

namespace Reusable.Tester
{
    public interface IEqualityComparerAssertExtensions { }

    public static class EqualityComparerAssertExtensions
    {
        public static void IsCanonical<T>(this IEqualityComparerAssertExtensions assert, IEqualityComparer<T> comparer, T other)
        {
            Assert.IsTrue(comparer.Equals(default(T), default(T)), CreateMessage("null == null"));
            Assert.IsFalse(comparer.Equals(default(T), other), CreateMessage("null != other"));
            Assert.IsFalse(comparer.Equals(other, default(T)), CreateMessage("other != null"));
            Assert.IsTrue(comparer.Equals(other, other), CreateMessage("other == other"));

            string CreateMessage(string requirement)
            {
                return $"{typeof(IEqualityComparer<T>).ToPrettyString()} violates the {requirement.QuoteWith("'")} requirement.";
            }
        }
    }
}
