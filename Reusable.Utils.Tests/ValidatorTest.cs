using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Reusable.Tests
{
    [TestClass]
    public class ValidatorTest
    {
        [TestMethod]
        [ExpectedException(typeof(CircularDependencyException))]
        public void ValidateDependencies_CircularDependencies_Throws()
        {
            var values = new Dictionary<string, IEnumerable<string>>
            {
                { "c", new string[] {  } },
                { "x", new string[] { "y" } },
                { "y", new string[] { "c", "z" } },
                { "z", new string[] { "x" } },
            };

            Validator.ValidateDependencies(values);
        }

        [TestMethod]
        [ExpectedException(typeof(MissingDependencyException))]
        public void ValidateDependencies_MissingDependencies_Throws()
        {
            var values = new Dictionary<string, IEnumerable<string>>
            {
                { "c", new string[] {  } },
                { "x", new string[] { "y" } },
                { "y", new string[] { "c", "z" } },
                { "z", new string[] { "a" } },
            };

            Validator.ValidateDependencies(values);
        }
    }
}
