using System.Linq;
using Xunit;

namespace Reusable.Commander
{
    public class ArgumentNameCollectionTest
    {
        [Fact]
        public void Can_sort_names()
        {
            var c = ArgumentNameCollection.Create("b", "c", "a");

            Assert.Equal(new[] { "b", "a", "c" }, c.Select(x => x.Value));
        }
    }
}