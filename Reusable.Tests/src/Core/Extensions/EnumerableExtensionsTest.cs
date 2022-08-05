using System;
using System.Collections.Generic;
using System.Linq.Custom;
using Reusable.Essentials;
using Reusable.Marbles;
using Xunit;

namespace Reusable.Extensions
{
    public class EnumerableExtensionsTest
    {
        public class SingleOrThrowTest
        {
            [Fact]
            public void Throws_when_collection_is_empty()
            {
                var x = Array.Empty<int>();
                Assert.ThrowsAny<DynamicException>(() => x.SingleOrThrow("x"));
            }

            [Fact]
            public void Throws_when_collection_contains_more_then_one_element()
            {
                var x = new[] { 1, 2 };
                Assert.ThrowsAny<DynamicException>(() => x.SingleOrThrow("x"));
            }
        }

        public class StartsWithTest
        {
            [Fact]
            public void Returns_True_when_first_collection_starts_with_second()
            {
                var x = new[] { 1, 2, 3, 4 };
                var y = new[] { 1, 2, 3 };

                Assert.True(x.StartsWith(y));
            }
        }

        [Fact]
        public void Diff_can_compare_collections()
        {
            var a = new (int id, string name)[] { (1, "a"), (2, "b"), (3, "d") };
            var b = new (int id, string name)[] { (2, "b"), (3, "e"), (4, "d") };

            var diff = a.Diff(b, x => x.id, x => x.name, EqualityComparer<int>.Default, EqualityComparer<string>.Default);

            Assert.Equal(new[] { (4, "d") }, diff.Added);
            Assert.Equal(new[] { (1, "a") }, diff.Removed);
            Assert.Equal(new[] { (2, "b") }, diff.Same);
            Assert.Equal(new[] { (3, "e") }, diff.Changed);
        }
    }

    public class ZipOrDefault
    {
        [Fact]
        public void Returns_default_when_end_of_collection()
        {
            var a = new[] { 1, 2 };
            var b = new[] { 2, 3, 4, 5 };

            Assert.Equal(new[] { (1, 2), (2, 3), (0, 4), (0, 5) }, a.ZipOrDefault(b));
            Assert.Equal(new[] { (2, 1), (3, 2), (4, 0), (5, 0) }, b.ZipOrDefault(a));
        }
    }
}