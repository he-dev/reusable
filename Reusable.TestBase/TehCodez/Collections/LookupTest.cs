using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.TestTools.UnitTesting.Infrastructure.Collections
{
    public abstract class LookupTest<TKey, TElement>
    {
        [TestMethod]
        public void Count_Empty_Zero()
        {
            Assert.IsFalse(GetEmptyLookup().Any());
        }

        [TestMethod]
        public void Count_NonEmpty_NumberOfElements()
        {
            Assert.IsTrue(GetNonEmptyLookup().Count > 0);
        }

        [TestMethod]
        public void Contains_KeyExists_True()
        {
            var lookup = GetNonEmptyLookup();
            foreach (var t in GetExistingKeys())
            {
                Assert.IsTrue(lookup.Contains(t.Key), $"Does not contain key '{t.Key}' but it should.");
            }
        }

        [TestMethod]
        public void Contains_KeyDoesNotExist_False()
        {
            var lookup = GetNonEmptyLookup();
            foreach (var t in GetNonExistingKeys())
            {
                Assert.IsFalse(lookup.Contains(t.Key), $"Contains key '{t.Key}' but shouldn't.");
            }
        }

        [TestMethod]
        public void this_KeyExists_Elements()
        {
            var lookup = GetNonEmptyLookup();
            foreach (var t in GetExistingKeys())
            {
                Assert.IsTrue(lookup[t.Key].SequenceEqual(t.Elements), $"Sequences for '{t.Key}' are not equal by they should.");
            }
        }

        [TestMethod]
        public void this_KeyDoesNotExist_Empty()
        {
            var lookup = GetNonEmptyLookup();
            foreach (var t in GetNonExistingKeys())
            {
                Assert.IsFalse(lookup[t.Key].Any(), $"There are some elements for '{t.Key}' but shouldn't.");
            }
        }

        protected abstract ILookup<TKey, TElement> GetEmptyLookup();

        protected abstract ILookup<TKey, TElement> GetNonEmptyLookup();

        protected abstract IEnumerable<(TKey Key, IEnumerable<TElement> Elements)> GetExistingKeys();

        protected abstract IEnumerable<(TKey Key, IEnumerable<TElement> Elements)> GetNonExistingKeys();
    }
}
