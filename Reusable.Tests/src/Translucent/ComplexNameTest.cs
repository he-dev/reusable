using Xunit;

namespace Reusable.Translucent
{
    public class ComplexNameTest
    {
        [Fact]
        public void Equal_when_empty()
        {
            Assert.Equal(ControllerName.Empty, ControllerName.Empty);
        }

        [Fact]
        public void Equal_when_primary_names_are_same()
        {
            Assert.Equal(new ControllerName("a"), new ControllerName("a"));
        }

        [Fact]
        public void Not_equal_when_primary_names_are_different()
        {
            Assert.NotEqual(new ControllerName("a"), new ControllerName("b"));
        }

        [Fact]
        public void Equal_when_tags_overlap()
        {
            Assert.Equal(new ControllerName("a", "b"), new ControllerName("B", "C"));
        }

        [Fact]
        public void Not_equal_when_tags_do_not_overlap()
        {
            Assert.NotEqual(new ControllerName("a"), new ControllerName("B", "C"));
        }
    }
}