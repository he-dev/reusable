using System.Linq;
using Xunit;

namespace Reusable.Tests.XUnit.Experimental
{
    using static InfiniteCounterPosition;

    public class InfiniteCounterTest
    {
        [Fact]
        public void Repeats_sequence()
        {
            Assert.Equal(new[] { 0, 1, 2, 0, 1, 2 }, new InfiniteCounter(3).AsEnumerable().Take(6).Select(x => x.Value));
        }

        [Fact]
        public void Names_each_position()
        {
            Assert.Equal(new[] { First, Intermediate, Last, First, Intermediate, Last }, new InfiniteCounter(3).AsEnumerable().Take(6).Select(x => x.Position));
        }
    }
}