using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Reflection;
using Reusable.Utilities.MSTest;
using Reusable.Validation;

namespace Reusable.Tests.Validation
{
    [TestClass]
    public class DuckValidatorTest
    {
        [TestMethod]
        public void IsValidWhen_SingleRule_True()
        {
            var validator = new DuckValidator<Person>(model => model.IsValidWhen(p => p.FirstName != null));
            var person = new Person { FirstName = "John" };
            Assert.IsTrue(validator.Validate(person).Success);
        }

        [TestMethod]
        public void IsValidWhen_SingleRule_False()
        {
            var validator = new DuckValidator<Person>(model => model.IsValidWhen(p => p.FirstName != null));
            var person = new Person();
            Assert.IsFalse(validator.Validate(person).Success);
        }

        [TestMethod]
        public void IsValidWhen_MultipleRules_True()
        {
            var validator = new DuckValidator<Person>(model =>
            {
                model
                    .IsValidWhen(p => p.FirstName != null)
                    .IsValidWhen(p => p.LastName != null);
            });

            var person = new Person { FirstName = "John", LastName = "Doe" };
            Assert.IsTrue(validator.Validate(person).Success);
        }

        [TestMethod]
        public void IsValidWhen_MultipleRules_False()
        {
            var validator = new DuckValidator<Person>(model =>
            {
                model
                    .IsValidWhen(p => p.FirstName != null)
                    .IsValidWhen(p => p.LastName != null);
            });

            var person = new Person();
            Assert.IsFalse(validator.Validate(person).Success);
        }

        [TestMethod]
        public void IsValidWhenNull_Null_True()
        {
            var validator = new DuckValidator<Person>(model => model.IsValidWhenNull());
            var person = default(Person);
            Assert.IsTrue(validator.Validate(person).Success);
        }

        [TestMethod]
        public void IsValidWhenNull_NotNull_False()
        {
            var validator = new DuckValidator<Person>(model => model.IsValidWhenNull());
            var person = new Person();
            Assert.IsFalse(validator.Validate(person).Success);
        }

        [TestMethod]
        public void IsNotValidWhenNull_NotNull_True()
        {
            var validator = new DuckValidator<Person>(model => model.IsNotValidWhenNull());
            var person = new Person();
            Assert.IsTrue(validator.Validate(person).Success);
        }

        [TestMethod]
        public void IsNotValidWhenNull_Null_False()
        {
            var validator = new DuckValidator<Person>(model => model.IsNotValidWhenNull());
            var person = default(Person);
            Assert.IsFalse(validator.Validate(person).Success);
        }

        //[TestMethod]
        //public void ValidateWith_MultipleRules_Validations()
        //{
        //    //var age = 5;
        //    //var lastName = "Doe";

        //    var validator = DuckValidator<Person>.Empty
        //        .IsNotValidWhen(p => p.FirstName == null);

        //    var person = new Person();
        //    var context = person.ValidateWith(validator);

        //    Assert.AreEqual("Not((<Person>.FirstName == null))", context.Results.ElementAt(0).Expression);
        //}

        [TestMethod]
        public void ThrowOrDefault_InvalidPerson_PersonValidationException()
        {
            var validator = new DuckValidator<Person>(model => model.IsNotValidWhen(p => p.FirstName == null));
            var person = new Person();

            Assert.That.Throws<DynamicException>(() => validator.Validate(person).ThrowOrDefault(), filter => filter.WhenName("PersonValidationException"));
        }

        public class Person
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public int Age { get; set; }

            public DateTime DayOfBirth { get; set; }
        }
    }
}
