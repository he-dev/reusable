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

//        [Fact]
//        public void Can_validate_rules()
//        {
//            var rules =
//                ValidationRuleCollection
//                    .For<Person>()
//                    .Add(x =>
//                        ValidationRule
//                            .Require
//                            .NotNull(x))
//                    .Add(x =>
//                        ValidationRule
//                            .Require
//                            .NotNull(() => x.FirstName))
//                    .Add(x =>
//                        ValidationRule
//                            .Ensure
//                            .True(() => x.FirstName.Length > 3))
//                    .Add(x =>
//                        ValidationRule
//                            .Require
//                            .NotNull(() => x.Address))
//                    .Add(x =>
//                        ValidationRule
//                            .Ensure
//                            .False(() => x.Address.Street.Length > 100));
//
//            var results = Tester.ValidateWith(rules);
//
//            Assert.Equal(5, results.OfType<Information>().Count());
//            Assert.Equal(0, results.OfType<Error>().Count());
//
//            Tester.ValidateWith(rules).ThrowIfValidationFailed();
//        }
//
//        [Fact]
//        public void Can_throw_if_validation_failed()
//        {
//            var rules =
//                ValidationRuleCollection
//                    .For<Person>()
//                    .Add(x =>
//                        ValidationRule
//                            .Require
//                            .NotNull(x))
//                    .Add(x =>
//                        ValidationRule
//                            .Require
//                            .NotNull(() => x.FirstName))
//                    .Add(x =>
//                        ValidationRule
//                            .Ensure
//                            .True(() => x.FirstName.Length > 3));
//
//            var results = default(Person).ValidateWith(rules);
//
//            Assert.Equal(0, results.OfType<Information>().Count());
//            Assert.Equal(1, results.OfType<Error>().Count());
//            
//            Assert.ThrowsAny<DynamicException>(() => default(Person).ValidateWith(rules).ThrowIfValidationFailed());
//        }
        
        [Fact]
        public void Simplified()
        {
            var rules =
                ValidationRuleCollection
                    .For<Person>()
                    .Add(b => b.NotNull(x => x).Require())
                    .Ensure(b => b.NotNull(x => x.FirstName))
                    .Ensure(b => b.True(x => x.FirstName.Length > 3));

            var results = default(Person).ValidateWith(rules);

            Assert.Equal(0, results.OfType<Information>().Count());
            Assert.Equal(1, results.OfType<Error>().Count());

            
            Assert.ThrowsAny<DynamicException>(() => default(Person).ValidateWith(rules).ThrowIfValidationFailed());
        }
        
        [Fact]
        public void Can_check_string_for_null_or_empty()
        {
            var rules =
                ValidationRuleCollection
                    .For<Person>()
                    .Add(b => b.NotNullOrEmpty(x => x.FirstName));

            var results = Tester.ValidateWith(rules);

            Assert.Equal(1, results.OfType<Information>().Count());
            Assert.Equal(0, results.OfType<Error>().Count());
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