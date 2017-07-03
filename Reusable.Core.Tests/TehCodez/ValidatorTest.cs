using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Tests
{
    [TestClass]
    public class ValidatorTest
    {
        [TestMethod]
        //[ExpectedException(typeof(CircularDependencyException))]
        public void ValidateDependencies_ValidDependencies_Passes()
        {
            var values = new Dictionary<string, IEnumerable<string>>
            {
                { "c", new string[] {  } },
                { "x", new string[] { "y" } },
                { "y", new string[] { "c", "z" } },
                { "z", new string[] { "c" } },
            };

            DependencyValidator.ValidateDependencies(values);
        }

        [TestMethod]
        [ExpectedException(typeof(CircularDependencyException))]
        public void ValidateDependencies_CircularDependencies_Fails()
        {
            var values = new Dictionary<string, IEnumerable<string>>
            {
                { "c", new string[] {  } },
                { "x", new string[] { "y" } },
                { "y", new string[] { "c", "z" } },
                { "z", new string[] { "x" } },
            };

            DependencyValidator.ValidateDependencies(values);
        }

        [TestMethod]
        [ExpectedException(typeof(MissingDependencyException))]
        public void ValidateDependencies_MissingDependencies_Fails()
        {
            var values = new Dictionary<string, IEnumerable<string>>
            {
                { "c", new string[] {  } },
                { "x", new string[] { "y" } },
                { "y", new string[] { "c", "z" } },
                { "z", new string[] { "a" } },
            };

            DependencyValidator.ValidateDependencies(values);
        }

        [TestMethod]
        public void MyTestMethod()
        {
            var numbers = new[] { 1, 2, 3 }.Select(x =>
            {
                Assert.Fail("Wrong number.");
                return x;
            });

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, numbers.ToList());

        }
    }
}
