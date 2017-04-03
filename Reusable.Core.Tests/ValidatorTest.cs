using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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

            var result = DependencyValidator.ValidateDependencies(values);
            Assert.IsTrue(result);
        }

        [TestMethod]
        //[ExpectedException(typeof(CircularDependencyException))]
        public void ValidateDependencies_CircularDependencies_Fails()
        {
            var values = new Dictionary<string, IEnumerable<string>>
            {
                { "c", new string[] {  } },
                { "x", new string[] { "y" } },
                { "y", new string[] { "c", "z" } },
                { "z", new string[] { "x" } },
            };

            var result = DependencyValidator.ValidateDependencies(values);
            Assert.IsFalse(result);
        }

        [TestMethod]
        //[ExpectedException(typeof(MissingDependencyException))]
        public void ValidateDependencies_MissingDependencies_Fails()
        {
            var values = new Dictionary<string, IEnumerable<string>>
            {
                { "c", new string[] {  } },
                { "x", new string[] { "y" } },
                { "y", new string[] { "c", "z" } },
                { "z", new string[] { "a" } },
            };

            var result = DependencyValidator.ValidateDependencies(values);
            Assert.IsFalse(result);
        }
    }
}
