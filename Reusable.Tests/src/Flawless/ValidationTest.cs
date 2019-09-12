using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.Collections;
using Reusable.Data;
using Reusable.Exceptionize;
using Xunit;

namespace Reusable.Flawless
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
//        public void Simplified()
//        {
//            var rules =
//                Validator
//                    .For<Person>()
//                    .Reject(b => b.Null(x => x).Required())
//                    .Reject(b => b.Null(x => x.FirstName))
//                    .Accept(b => b.When(x => x.FirstName.Length > 3));
//
//            var results = default(Person).ValidateWith(rules);
//
//            Assert.Equal(0, results.Successful().Count());
//            Assert.Equal(1, results.Errors().Count());
//
//            
//            Assert.ThrowsAny<DynamicException>(() => default(Person).ValidateWith(rules).ThrowOnFailure());
//        }

        [Fact]
        public void Simplified_2()
        {
            var personValidator = Validator.Validate<Person>(person =>
            {
                //person.Not().Null().Error();
                // or
                //person.Required();

                person.Validate(x => x.FirstName).Required();
                //person.Validate(x => x.FirstName).Not().Null().Required().Like(@"^[a-z]+");
                var rule = person.ValidateAll(x => x.Emails).Not().NullOrEmpty().Build().ToList();

                var results2 = rule.First().Validate(new Person { Emails = new List<string> { "blub" } }, ImmutableContainer.Empty).ToList();

                person.Validate(x => x.FirstName, firstName =>
                {
                    //firstName.Required();
                    //firstName.Not().NullOrEmpty();
                    firstName.Validate(y => y.Length).GreaterThan(0);
                    firstName.When(x => x.LastName != null && x.LastName.StartsWith("S")).Equal("Sam"); //.Message("It must be Sam.");
                    firstName.Like(@"^[a-z]+");
                });

                person.Validate(x => x.Address, address =>
                {
                    address.Required();
                    address.Validate(x => x.Street, street => { street.Required().Message("You need to specify the Street."); });
                });
            });


            var dto = new Person
            {
                FirstName = "Joe",
                Address = new Address(),
                Emails = new List<string> { "blub" }
            };

            var results = personValidator.Validate(dto).ToList();

//
//            Assert.Equal(0, results.Successful().Count());
//            Assert.Equal(1, results.Errors().Count());
//
//            personValidator.Validate(default(Person), default);
//
//            
//            Assert.ThrowsAny<DynamicException>(() => default(Person).ValidateWith(personValidator).ThrowOnFailure());
        }

        [Fact]
        public void Can_validate_collections_1()
        {
            var personValidator = Validator.Validate<Person>(p =>
            {
                p.Validate(x => x.Emails).Required();
                p.ValidateAll(x => x.Emails).Not().Null();
            });

            var person = new Person
            {
                Emails = new List<string> { "a", "b" }
            };

            var results = person.ValidateWith(personValidator).ToList();

            Assert.Equal(2, results.Count);
            Assert.Equal(2, results.OfType<ValidationSuccess>().Count());
        }

        [Fact]
        public void Can_validate_collections_2()
        {
            var personValidator = Validator.Validate<Person>(p =>
            {
                p.Validate(x => x.Emails).Required();
                p.ValidateAll(x => x.Emails).Not().Null();
            });

            var person = new Person
            {
                Emails = new List<string> { "a", null }
            };

            var results = person.ValidateWith(personValidator).ToList();

            Assert.Equal(2, results.Count);
            Assert.Equal(1, results.OfType<ValidationSuccess>().Count());
            Assert.Equal(1, results.OfType<ValidationWarning>().Count());
        }

//        [Fact]
//        public void Can_check_string_for_null_or_empty()
//        {
//            var rules =
//                Validator
//                    .For<Person>()
//                    .Reject(b => b.NullOrEmpty(x => x.FirstName).Required())
//                    .Accept(b => b.Equal(x => x.FirstName, "cookie", StringComparer.OrdinalIgnoreCase));
//
//            var results = Tester.ValidateWith(rules);
//
//            Assert.Equal(2, results.Successful().Count());
//            Assert.Equal(0, results.Errors().Count());
//        }
//        
//        [Fact]
//        public void Can_match_string()
//        {
//            var rules =
//                Validator
//                    .For<Person>()
//                    .Accept(b => b.Like(x => x.FirstName, "^cookie", RegexOptions.IgnoreCase))
//                    .Reject(b => b.Like(x => x.FirstName, "^cookie", RegexOptions.IgnoreCase).Required());
//
//            var results = Tester.ValidateWith(rules);
//
//            Assert.Equal(1, results.OfType<ValidationSuccess>().Count());
//            Assert.Equal(1, results.OfType<ValidationError>().Count());
//
//            var ex = Assert.ThrowsAny<DynamicException>(() => results.ThrowOnFailure());
//        }
    }

    internal class Person
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public Address Address { get; set; }

        public IEnumerable<string> Emails { get; set; }
    }

    internal class Address
    {
        public string Street { get; set; }
    }

    internal class AddressValidatorModule : IValidatorModule<Address>
    {
        public void Build(IValidationRuleBuilder<Address> builder)
        {
            throw new NotImplementedException();
        }
    }
}