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
            Assert.Equal(TestOption.B, TestOption.B);
            Assert.NotEqual(TestOption.B, TestOption.C);
        }

        [Fact]
        public void Can_set_flags()
        {
            var actual = TestOption.C | TestOption.B | TestOption.A;
            Assert.True(actual.Contains(TestOption.A));
            Assert.True(actual.Contains(TestOption.B));
            Assert.True(actual.Contains(TestOption.C));

            Assert.Equal("A, B, C", actual.ToString());
        }

        [Fact]
        public void Can_reset_flags()
        {
            var actual = TestOption.A.RemoveFlag(TestOption.A);
            Assert.False(actual.Contains(TestOption.A));
            Assert.Equal(TestOption.None, actual);
        }

        private class TestOption : Option<TestOption>
        {
            public TestOption(SoftString name, IImmutableSet<SoftString> values) : base(name, values) { }

            public static readonly TestOption A = CreateWithCallerName();
            public static readonly TestOption B = CreateWithCallerName();
            public static readonly TestOption C = CreateWithCallerName();
            public static readonly TestOption D = Create("2D");
        }

        private class TestOptions
        {
            public static readonly TestOptions A = Option<TestOptions>.CreateWithCallerName();
        }
    }
}