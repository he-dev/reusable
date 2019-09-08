using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.Collections;
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
//            var personValidator =
//                Validator<Person>
//                    .Empty
//                    .For(x => x, r => r.Not().Null().Required())
//                    .For(x => x.FirstName, r => r.Not().Null().Required()); // --> ValidationRuleBuilder<T, TContext>
//            

            var personValidator = Validator.Validate<Person>(person =>
            {
                //person.Not().Null().Error();
                // or
                //person.Required();

                //person.Validate(x => x.FirstName).Required();
                //person.Validate(x => x.FirstName).Not().Null().Required().Like(@"^[a-z]+");
                person.Validate(x => x.FirstName, firstName =>
                {
                    //firstName.Required();
                    //firstName.Not().NullOrEmpty();
                    firstName.Validate(y => y.Length).GreaterThan(0);
                    firstName.When(x => x.StartsWith("S")).Equal("Sam").Message("It must be Sam.");
                    firstName.Like(@"^[a-z]+");
                });

                person.Validate(x => x.Address, address =>
                {
                    address.Required();
                    address.Validate(x => x.Street, street =>
                    {
                        street.Required().Message("You need to specify the Street.");
                    });
                });
            });

            //validator.When(x => x.LastName).Null().Required();


            var results = new Person { FirstName = "Joe", Address = new Address() }.ValidateWith(personValidator).ToList();
//
//            Assert.Equal(0, results.Successful().Count());
//            Assert.Equal(1, results.Errors().Count());
//
//            personValidator.Validate(default(Person), default);
//
//            
//            Assert.ThrowsAny<DynamicException>(() => default(Person).ValidateWith(personValidator).ThrowOnFailure());
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

        private class Person
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public Address Address { get; set; }

            public IEnumerable<string> Emails { get; set; }
        }

        private class Address
        {
            public string Street { get; set; }
        }
    }
}