using System.Linq;
using Reusable.Flawless;
using Xunit;

namespace Reusable.Tests.Flawless
{
    public class Test
    {
        [Fact]
        public void asdf()
        {
            var rules =
                ValidationRuleCollection
                    .For<Person>()
                    .Add((x, _) =>
                        ValidationRule
                            .Require(x)
                            .NotNull())
                    .Add((x, _) =>
                        ValidationRule
                            .Ensure(x)
                            .True(() => x.FirstName.Length > 0));

            var p = new Person { FirstName = "Bob" };

            var results = p.ValidateWith(rules, default).ToList();
            
        }

        private class Person
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }
        }
    }
}