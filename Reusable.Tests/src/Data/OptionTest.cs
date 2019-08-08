using System.Collections.Immutable;
using Reusable.Beaver;
using Xunit;

namespace Reusable.Data
{
    public class OptionTest
    {
        [Fact]
        public void Can_compare_options_for_equality()
        {
            Assert.Equal(TestOption.Two, TestOption.Two);
            Assert.NotEqual(TestOption.Two, TestOption.Three);
        }

        [Fact]
        public void Can_set_flags()
        {
            var actual = TestOption.One | TestOption.Two | TestOption.Three;
            Assert.True(actual.Contains(TestOption.One));
            Assert.True(actual.Contains(TestOption.Two));
            Assert.True(actual.Contains(TestOption.Three));
        }
        
        [Fact]
        public void Can_reset_flags()
        {
            var actual = TestOption.One.RemoveFlag(TestOption.One);
            Assert.False(actual.Contains(TestOption.One));
            Assert.Equal(TestOption.None, actual);
        }

        private class TestOption : Option<TestOption>
        {
            public TestOption(SoftString name, IImmutableSet<SoftString> values) : base(name, values) { }

            public static readonly TestOption One = CreateWithCallerName();
            public static readonly TestOption Two = CreateWithCallerName();
            public static readonly TestOption Three = CreateWithCallerName();
            public static readonly TestOption Custom = Create("Other");
        }
    }
}