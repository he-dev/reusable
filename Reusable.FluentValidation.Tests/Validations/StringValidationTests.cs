using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Validations;

namespace Reusable.FluentValidation.Tests.Validations
{
    [TestClass]
    public class StringValidationTests
    {

        [TestMethod]
        public void IsNullOrEmpty_True()
        {
            ((string)null).Validate().IsNullOrEmpty();
            string.Empty.Validate().IsNullOrEmpty();
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void IsNullOrEmpty_False()
        {
            "foo".Validate().IsNullOrEmpty();
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void IsMatch_False()
        {
            "foo".Validate().IsMatch("^[a-z]$");
        }

        [TestMethod]
        public void IsNotMatch_True()
        {
            "foo".Validate().IsNotMatch("[0-9]+");
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void IsNotMatch_False()
        {
            "123".Validate().IsNotMatch("[0-9]+");
        }
    }
}
