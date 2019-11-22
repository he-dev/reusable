using Xunit;

namespace Reusable.Data
{
    public class OptionTest
    {
        [Fact]
        public void Can_compare_options_for_equality()
        {
            Assert.Equal(Test.Options.B, Test.Options.B);
            Assert.NotEqual(Test.Options.B, Test.Options.C);
        }

        [Fact]
        public void Can_set_flags()
        {
            var actual = Test.Options.C | Test.Options.B | Test.Options.A;
            Assert.True(actual.Contains(Test.Options.A));
            Assert.True(actual.Contains(Test.Options.B));
            Assert.True(actual.Contains(Test.Options.C));

            Assert.Equal("A, B, C", actual.ToString());
        }

        [Fact]
        public void Can_reset_flags()
        {
            var actual = Test.Options.A.RemoveFlag(Test.Options.A);
            Assert.False(actual.Contains(Test.Options.A));
            Assert.Equal(Option<Test>.None, actual);
        }

        private class Test
        {
            public static class Options
            {
                public static readonly Option<Test> A = Option<Test>.CreateWithCallerName();
                public static readonly Option<Test> B = Option<Test>.CreateWithCallerName();
                public static readonly Option<Test> C = Option<Test>.CreateWithCallerName();
                public static readonly Option<Test> D = Option<Test>.Create("2D");
            }
        }
        
    }
}