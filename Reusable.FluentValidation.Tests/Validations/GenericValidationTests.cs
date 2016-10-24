using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.FluentValidation;
using Reusable.FluentValidation.Validations;

namespace SmartUtilities.Tests.Unit.Frameworks.InlineValidation.Validations
{
    [TestClass]
    public class GenericValidationTests
    {
        [TestMethod]
        public void IsNull_True()
        {
            ((object)null).Validate().IsNull();
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void IsNull_False()
        {
            new object().Validate().IsNull();
        }

        [TestMethod]
        public void IsLessThen_True()
        {
            1.Validate().IsLessThen(2);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void IsLessThen_EqualValue_False()
        {
            1.Validate().IsLessThen(1);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void IsLessThen_GreaterValue_False()
        {
            2.Validate().IsLessThen(1);
        }

        [TestMethod]
        public void IsLessThenOrEqual_True()
        {
            1.Validate().IsLessThenOrEqual(2);
            1.Validate().IsLessThenOrEqual(1);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void IsLessThenOrEqual_False()
        {
            1.Validate().IsLessThenOrEqual(0);
        }

        [TestMethod]
        public void IsGreaterThen_True()
        {
            1.Validate().IsGreaterThen(0);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void IsGreaterThen_EqualValue_False()
        {
            1.Validate().IsGreaterThen(1);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void IsGreaterThen_GreaterValue_False()
        {
            1.Validate().IsGreaterThen(2);
        }

        [TestMethod]
        public void IsGreaterThenOrEqual_True()
        {
            1.Validate().IsGreaterThenOrEqual(1);
            1.Validate().IsGreaterThenOrEqual(0);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void IsGreaterThenOrEqual_False()
        {
            1.Validate().IsGreaterThenOrEqual(2);
        }

        [TestMethod]
        public void IsBetween_True()
        {
            1.Validate().IsBetween(0, 2);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void IsBetween_EqualValue_False()
        {
            1.Validate().IsBetween(1, 2);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void IsBetween_LessValue_False()
        {
            4.Validate().IsBetween(2, 4);
        }

        [TestMethod]
        public void IsBetweenOrEqua_True()
        {
            1.Validate().IsBetweenOrEqual(0, 2);
            1.Validate().IsBetweenOrEqual(1, 2);
            1.Validate().IsBetweenOrEqual(0, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void IsBetweenOrEqual_False()
        {
            1.Validate().IsBetweenOrEqual(2, 3);
        }

        [TestMethod]
        public void IsEqual_True()
        {
            1.Validate().IsEqual(1);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void IsEqual_False()
        {
            1.Validate().IsEqual(2);
        }

        [TestMethod]
        public void IsNotEqual_True()
        {
            1.Validate().IsNotEqual(0);
            1.Validate().IsNotEqual(2);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void IsNotEqual_False()
        {
            1.Validate().IsNotEqual(1);
        }

        [TestMethod]
        public void IsMatch_True()
        {
            "foo".Validate().IsMatch("[a-z]+");
        }

        [TestMethod]
        public void IsInstanceOfType_True()
        {
            new string[] { }.Validate().IsInstanceOfType(typeof(string[]));
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void IsInstanceOfType_False()
        {
            new string[] { }.Validate().IsInstanceOfType(typeof(int[]));
        }

        //[TestMethod]
        //public void IsAssignableFrom_True()
        //{
        //    new Action(() => ((IEnumerable<string>)new string[] { }).Validate().IsAssignableFrom(typeof(string[]))).Test().DoesNotThrow();
        //}

        //[TestMethod]
        //public void IsAssignableFrom_False()
        //{
        //    new Action(() => new string[] { }.Validate().IsAssignableFrom(typeof(int[]))).Test()._False<ArgumentException>();
        //}
    }
}
