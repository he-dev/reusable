using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Extensions;

namespace Reusable.Utilities.MSTest
{
    public interface ICollectionAssert { }

    public static class CollectionAssertExtensions
    {
        public static void IsEmpty<T>(this ICollectionAssert assert, [CanBeNull] IEnumerable<T> collection)
        {
            Assert.IsNotNull(collection, $"Collection of {typeof(T).Name.QuoteWith("'")} must not be null.");
            Assert.IsFalse(collection.Any(), $"Collection of {typeof(T).Name.QuoteWith("'")} must be empty.");
        }

        public static void IsNotEmpty<T>(this ICollectionAssert assert, [CanBeNull] IEnumerable<T> collection)
        {
            Assert.IsNotNull(collection, $"Collection of {typeof(T).Name.QuoteWith("'")} must not be null.");
            Assert.IsTrue(collection.Any(), $"Collection of {typeof(T).Name.QuoteWith("'")} must not be empty.");
        }

        public static void CountEquals<T>(this ICollectionAssert assert, int expectedCount, IEnumerable<T> collection)
        {
            Assert.IsNotNull(collection, $"Collection of {typeof(T).Name.QuoteWith("'")} must not be null.");

            var actualCount = collection.Count();
            Assert.AreEqual(expectedCount, actualCount, $"Collection of {typeof(T).Name.QuoteWith("'")} must have {expectedCount} element(s) but found {actualCount}.");
        }

        public static void AreEqual<T>(this ICollectionAssert assert, IEnumerable<T> excpected, IEnumerable<T> actual)
        {
            var index = 0;
            foreach (var zip in excpected.ZipOrDefault(actual, (left, right) => (left, right)))
            {
                if (!zip.left.Equals(zip.right))
                {
                    Assert.Fail($"Elements at {index} don't match: {zip.left?.ToString()} != {zip.right?.ToString()}");
                }
                index++;
            }
        }
    }
}