using System.Collections.Generic;
using System.Linq.Custom;
using Reusable.Exceptionize;
using Xunit;

namespace Reusable.Tests.Extensions
{
    public static class EnumerableExtensionsTest
    {
        public class SingleOrThrowTest
        {
            [Fact]
            public void Throws_when_collection_is_empty()
            {
                var x = new int[0];
                Assert.ThrowsAny<DynamicException>(() => x.SingleOrThrow());
            }

            [Fact]
            public void Throws_when_collection_contains_more_then_one_element()
            {
                var x = new[] { 1, 2 };
                Assert.ThrowsAny<DynamicException>(() => x.SingleOrThrow());
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
    }
}