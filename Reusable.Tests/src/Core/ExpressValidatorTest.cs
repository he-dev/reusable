using Reusable.Flawless;
using Xunit;

namespace Reusable.Tests
{
    public class ExpressValidatorTest
    {
        [Fact]
        public void CanAssertNull()
        {
            var validator = ExpressValidator.For<User>(assert =>
            {
                assert.Null(x => x.Name);
            });
            var results = validator.Validate(new User());

            Assert.True(results.Success);
        }

        [Fact]
        public void CanAssertNotNull()
        {
            var validator = ExpressValidator.For<User>(assert =>
            {
                assert.NotNull(x => x.Name);
            });
            var results = validator.Validate(new User());

            Assert.False(results.Success);
        }


        private class User
        {
            public string Name { get; set; }
        }
    }
}