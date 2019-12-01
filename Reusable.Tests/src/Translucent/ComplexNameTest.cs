using Xunit;

namespace Reusable.Translucent
{
    public class ComplexNameTest
    {
        [Fact]
        public void Equal_when_empty()
        {
            Assert.Equal(ComplexName.Empty, ComplexName.Empty);
        }

        [Fact]
        public void Equal_when_primary_names_are_same()
        {
            Assert.Equal(new ComplexName("a"), new ComplexName("a"));
        }

        [Fact]
        public void Not_equal_when_primary_names_are_different()
        {
            Assert.NotEqual(new ComplexName("a"), new ComplexName("b"));
        }

        [Fact]
        public void Equal_when_tags_overlap()
        {
            Assert.Equal(new ComplexName { "a", "b" }, new ComplexName { "B", "C" });
        }

        [Fact]
        public void Not_equal_when_tags_do_not_overlap()
        {
            Assert.NotEqual(new ComplexName { "a" }, new ComplexName { "B", "C" });
        }
    }
}