using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Reusable.Reflection;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests
{
    [TestClass]
    public class DependencyValidatorTest
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
        public void ValidateDependencies_CircularDependencies_Fails()
        {
            var values = new Dictionary<string, IEnumerable<string>>
            {
                { "c", new string[] {  } },
                { "x", new string[] { "y" } },
                { "y", new string[] { "c", "z" } },
                { "z", new string[] { "x" } },
            };

            Assert.That.ThrowsExceptionFiltered<DynamicException>(() => DependencyValidator.ValidateDependencies(values), ex => ex.NameEquals("CircularDependencyException"));
        }

        [TestMethod]
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

            Assert.That.ThrowsExceptionFiltered<DynamicException>(() => DependencyValidator.ValidateDependencies(values), ex => ex.NameEquals("MissingDependencyException"));
        }
    }
}
