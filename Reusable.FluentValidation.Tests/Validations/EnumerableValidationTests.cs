using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Validations;

namespace Reusable.FluentValidation.Tests.Validations
{
    [TestClass]
    public class EnumerableValidationTests
    {
        [TestMethod]
        public void IsEmpty_True()
        {
            new string[] { }.Validate().IsEmpty();
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void IsEmpty_False()
        {
            new string[] { "foo" }.Validate().IsEmpty();
        }

        [TestMethod]
        public void IsNotEmpty_True()
        {
            new string[] { "foo" }.Validate().IsNotEmpty();
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void IsNotEmpty_False()
        {
            new string[] { }.Validate().IsNotEmpty();
        }

        [TestMethod]
        public void Contains_True()
        {
            new string[] { "foo", "bar", "baz" }.Validate().Contains("BAR", StringComparer.OrdinalIgnoreCase);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void Contains_False()
        {
            new string[] { "foo", "bar", "baz" }.Validate().Contains("QUX", StringComparer.OrdinalIgnoreCase);
        }

        [TestMethod]
        public void DoesNotContain_True()
        {
            new string[] { "foo", "bar", "baz" }.Validate().DoesNotContain("qux", StringComparer.OrdinalIgnoreCase);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void DoesNotContain_False()
        {
            new string[] { "foo", "bar", "baz" }.Validate().DoesNotContain("baz", StringComparer.OrdinalIgnoreCase);
        }
    }
}
