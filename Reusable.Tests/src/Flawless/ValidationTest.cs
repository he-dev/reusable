using System;
using System.Linq;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Flawless;
using Xunit;

namespace Reusable.Tests.Flawless
{
    public class ValidationTest
    {
        private static readonly Person Tester = new Person
        {
            FirstName = "Cookie",
            LastName = "Monster",
            Address = new Address
            {
                Street = "Sesame Street"
            }
        };

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
                            .True(() => x.FirstName.Length > 3))
                    .Add(x =>
                        ValidationRule
                            .Require
                            .NotNull(() => x.Address))
                    .Add(x =>
                        ValidationRule
                            .Ensure
                            .False(() => x.Address.Street.Length > 100));

            var (person, results) = Tester.ValidateWith(rules);

            Assert.Equal(5, results[true].Count());
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
        public void Simplified()
        {
            var rules =
                ValidationRuleCollection
                    .For<Person>()
                    .Require((b, x) => b.NotNull(() => x))
                    .Ensure((b, x) => b.NotNull(() => x.FirstName))
                    .Ensure((b, x) => b.True(() => x.FirstName.Length > 3));

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

            public Address Address { get; set; }
        }

        private class Address
        {
            public string Street { get; set; }
        }
    }
}