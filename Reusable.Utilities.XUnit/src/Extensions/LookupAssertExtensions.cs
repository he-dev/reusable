using System.Linq;
using Reusable.Extensions;

namespace Reusable.Utilities.MSTest
{
    public interface ILookupAssert { }

    public static class LookupAssertExtensions
    {
        public static void HasCount<TKey, TElement>(this ILookupAssert assert, ILookup<TKey, TElement> lookup, int expectedCount)
        {
            Assert.AreEqual(expectedCount, lookup.Count, $"{typeof(ILookup<TKey, TElement>).ToPrettyString()}'s {nameof(ILookup<TKey, TElement>.Count)} failed.");
        }

        public static void ItemNotNull<TKey, TElement>(this ILookupAssert assert, ILookup<TKey, TElement> lookup, TKey existingKey, TKey nonExistingKey)
        {
            Assert.IsNotNull(lookup[existingKey], CreateMessage<TKey, TElement>("Item[existingKey] != null"));
            Assert.IsNotNull(lookup[nonExistingKey], CreateMessage<TKey, TElement>("Item[nonExistingKey] != null"));
        }

        public static void Contains<TKey, TElement>(this ILookupAssert assert, ILookup<TKey, TElement> lookup, TKey existingKey, TKey nonExistingKey)
        {
            Assert.IsTrue(lookup.Contains(existingKey), CreateMessage<TKey, TElement>($"Contains(existingKey) == {bool.TrueString}"));
            Assert.IsFalse(lookup.Contains(nonExistingKey), CreateMessage<TKey, TElement>($"Contains(nonExistingKey) == {bool.FalseString}"));
        }

        public static void ContainsMany<TKey, TElement>(this ILookupAssert assert, ILookup<TKey, TElement> lookup, params TKey[] keys)
        {
            foreach (var key in keys)
            {
                Assert.IsTrue(lookup.Contains(key), CreateMessage<TKey, TElement>($"Contains({key.Stringify()}) == {bool.TrueString}"));
            }
        }

        public static void DoesNotContainMany<TKey, TElement>(this ILookupAssert assert, ILookup<TKey, TElement> lookup, params TKey[] keys)
        {
            foreach (var key in keys)
            {
                Assert.IsFalse(lookup.Contains(key), CreateMessage<TKey, TElement>($"Contains({key.Stringify()}) == {bool.FalseString}"));
            }
        }

        private static string CreateMessage<TKey, TElement>(string requirement)
        {
            return $"{typeof(ILookup<TKey, TElement>).ToPrettyString()} violates the {requirement.QuoteWith("'")} requirement.";
        }
    }
}