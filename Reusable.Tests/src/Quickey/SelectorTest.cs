using Xunit;

namespace Reusable.Quickey
{
    [UseType, UseMember]
    public class SelectorTest
    {
        [Fact]
        public void blub()
        {
            Assert.Equal("SelectorTest.Greeting", Greeting);
        }

        public static readonly string Greeting = Selector.For(() => Greeting).ToString();
    }
}