using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Exceptionize;
using Reusable.Flawless;
using Reusable.Tester;

namespace Reusable.Tests.Flawless
{
    [TestClass]
    public class ValidatorTest
    {
        //[TestMethod]
        public void TestMethod1()
        {
            var age = 5;
            var lastName = "Doe";

            var personValidator = Validator<Person>.Empty
                .IsNotValidWhen(p => p == null, ValidationOptions.StopOnFailure)
                .IsValidWhen(p => !string.IsNullOrEmpty(p.FirstName))
                .IsNotValidWhen(p => p.LastName == null)
                //.Where(p => p.LastName != null)
                //.Where(p => !p.LastName.StartsWith("D"))
                .IsNotValidWhen(p => p.LastName.StartsWith("D"))
                .IsValidWhen(p => p.LastName != null)
                .IsValidWhen(p => p.LastName == lastName)
                .IsValidWhen(p => p.DayOfBirth == DateTime.Today)
                .IsValidWhen(p => p.Age > age);

            var person = new Person
            {
                FirstName = "John",
                LastName = "Doe"
            };

            personValidator.Validate(person);

            person.ValidateWith(personValidator).AllSuccess();
            default(Person).ValidateWith(personValidator);

            //person.ThrowIfInvalid(personValidator);
        }

        [TestMethod]
        public void Count_SingleRule_One()
        {
            var validator = Validator<Person>.Empty.IsValidWhen(p => p.FirstName != null);
            Assert.That.Collection().CountEquals(1, validator);
        }

        [TestMethod]
        public void Count_ThreeRules_Three()
        {
            var validator = Validator<Person>.Empty.IsValidWhen(p => p.FirstName != null).IsValidWhen(p => p.FirstName == "John").IsNotValidWhen(p => p.LastName == null);
            Assert.That.Collection().CountEquals(3, validator);
        }

        [TestMethod]
        public void IsValidWhen_SingleRule_True()
        {
            var validator = Validator<Person>.Empty.IsValidWhen(p => p.FirstName != null);
            var person = new Person { FirstName = "John" };
            Assert.IsTrue(person.ValidateWith(validator).AllSuccess());
        }

        [TestMethod]
        public void IsValidWhen_SingleRule_False()
        {
            var validator = Validator<Person>.Empty.IsValidWhen(p => p.FirstName != null);
            var person = new Person();
            Assert.IsFalse(person.ValidateWith(validator).AllSuccess());
        }

        [TestMethod]
        public void IsValidWhen_MultipleRules_True()
        {
            var validator = Validator<Person>.Empty.IsValidWhen(p => p.FirstName != null).IsValidWhen(p => p.LastName != null);
            var person = new Person { FirstName = "John", LastName = "Doe" };
            Assert.IsTrue(person.ValidateWith(validator).AllSuccess());
        }

        [TestMethod]
        public void IsValidWhen_MultipleRules_False()
        {
            var validator = Validator<Person>.Empty.IsValidWhen(p => p.FirstName != null).IsValidWhen(p => p.LastName != null);
            var person = new Person();
            Assert.IsFalse(person.ValidateWith(validator).AllSuccess());
        }

        [TestMethod]
        public void ValidateWith_MultipleRules_Validations()
        {
            var age = 5;
            var lastName = "Doe";

            var validator = Validator<Person>.Empty
                .IsNotValidWhen(p => p.FirstName == null);

            var person = new Person();
            var validations = person.ValidateWith(validator).ToList();

            Assert.AreEqual("Not((<Person>.FirstName == null))", validations.ElementAt(0).Expression);
        }

        [TestMethod]
        public void ThrowIfInvalid_InvalidPerson_PersonValidationException()
        {
            var validator = Validator<Person>.Empty.IsNotValidWhen(p => p.FirstName == null);
            var person = new Person();

            var ex = Assert.That.ThrowsExceptionFiltered<DynamicException>(() => person.ValidateWith(validator).ThrowIfNotValid(), e => e.NameEquals("PersonValidationException"));
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
