using System.Collections.Generic;
using System.Linq.Custom;
using Xunit;

namespace Reusable.Tests.XUnit.Extensions
{
    public class EnumerableExtensionsTest
    {
        [Fact]
        public void asdf()
        {
            var numbers = new List<int> { 1, 2, 3 };
            //numbers.SingleOrThrow();
        }
    }
}