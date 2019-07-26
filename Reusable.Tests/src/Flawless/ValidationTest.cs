using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        public void Simplified()
        {
            var rules =
                ValidationRuleCollection
                    .For<Person>()
                    .Reject(b => b.Null(x => x).Hard())
                    .Reject(b => b.Null(x => x.FirstName))
                    .Accept(b => b.When(x => x.FirstName.Length > 3));

            var results = default(Person).ValidateWith(rules);

            Assert.Equal(0, results.OfType<ValidationSuccess>().Count());
            Assert.Equal(1, results.OfType<ValidationError>().Count());

            
            Assert.ThrowsAny<DynamicException>(() => default(Person).ValidateWith(rules).ThrowOnFailure());
        }
        
        [Fact]
        public void Can_check_string_for_null_or_empty()
        {
            var rules =
                ValidationRuleCollection
                    .For<Person>()
                    .Reject(b => b.NullOrEmpty(x => x.FirstName).Hard())
                    .Accept(b => b.Equal(x => x.FirstName, "cookie", StringComparer.OrdinalIgnoreCase));

            var results = Tester.ValidateWith(rules);

            Assert.Equal(2, results.OfType<ValidationSuccess>().Count());
            Assert.Equal(0, results.OfType<ValidationError>().Count());
        }
        
        [Fact]
        public void Can_match_string()
        {
            var rules =
                ValidationRuleCollection
                    .For<Person>()
                    .Accept(b => b.Like(x => x.FirstName, "^cookie", RegexOptions.IgnoreCase))
                    .Reject(b => b.Like(x => x.FirstName, "^cookie", RegexOptions.IgnoreCase).Hard());

            var results = Tester.ValidateWith(rules);

            Assert.Equal(1, results.OfType<ValidationSuccess>().Count());
            Assert.Equal(1, results.OfType<ValidationError>().Count());
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