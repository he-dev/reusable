using System;
using System.Linq;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Flawless;
using Xunit;

namespace Reusable.Tests.Flawless
{
    public class Test
    {
        private static readonly Person Tester = new Person { FirstName = "John" };

        [Fact]
        public void Can_validate_rules()
        {
            var rules =
                ValidationRuleCollection
                    .For<Person>()
                    .Add(x =>
                        ValidationRule
                            .Require
                            .NotNull(x))
                    .Add(x =>
                        ValidationRule
                            .Require
                            .NotNull(() => x.FirstName))
                    .Add(x =>
                        ValidationRule
                            .Ensure
                            .True(() => x.FirstName.Length > 3));

            var (person, results) = Tester.ValidateWith(rules);
            
            Assert.Equal(3, results[true].Count());
            Assert.Equal(0, results[false].Count());
            
            Tester.ValidateWith(rules).ThrowIfValidationFailed();
        }

        [Fact]
        public void Can_throw_if_validation_failed()
        {
            var rules =
                ValidationRuleCollection
                    .For<Person>()
                    .Add(x =>
                        ValidationRule
                            .Require
                            .NotNull(x))
                    .Add(x =>
                        ValidationRule
                            .Require
                            .NotNull(() => x.FirstName))
                    .Add(x =>
                        ValidationRule
                            .Ensure
                            .True(() => x.FirstName.Length > 3));

            var (person, results) = default(Person).ValidateWith(rules);

            Assert.Equal(0, results[true].Count());
            Assert.Equal(1, results[false].Count());
            Assert.ThrowsAny<DynamicException>(() => default(Person).ValidateWith(rules).ThrowIfValidationFailed());
        }


        [Fact]
        public void asdf()
        {
            var rules =
                ValidationRuleCollection
                    .For<Person>()
                    .Add((x, _) =>
                        ValidationRule
                            .Require
                            .NotNull(x))
                    .Add((x, _) =>
                        ValidationRule
                            .Ensure
                            .True(() => x.FirstName.Length > 0));

            var p = new Person { FirstName = "Bob" };

            var results = p.ValidateWith(rules).Results.ToList();

            p.ValidateWith(rules).ThrowIfValidationFailed();
        }

        private class Person
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }
        }
    }
}