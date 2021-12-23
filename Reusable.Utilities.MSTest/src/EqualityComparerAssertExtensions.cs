using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Essentials.Extensions;

namespace Reusable.Utilities.MSTest
{
    public interface IEqualityComparerAssertExtensions { }

    public static class EqualityComparerAssertExtensions
    {
        public static void IsCanonical<T>(this IEqualityComparerAssertExtensions assert, IEqualityComparer<T> comparer, T value)
        {
            Assert.IsTrue(comparer.Equals(default, default), CreateMessage("null == null"));
            Assert.IsFalse(comparer.Equals(default, value), CreateMessage("null != value"));
            Assert.IsFalse(comparer.Equals(value, default), CreateMessage("value != null"));
            Assert.IsTrue(comparer.Equals(value, value), CreateMessage("value == value"));

            string CreateMessage(string requirement) => $"{typeof(IEqualityComparer<T>).ToPrettyString()} violates the {requirement.QuoteWith("'")} requirement.";
        }
    }
}
