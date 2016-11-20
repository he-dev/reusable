using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Testing;
using Reusable.Validations;

namespace Reusable.Collections.Tests
{
    [TestClass]
    public class FibonacciSequenceTest
    {
        [TestMethod]
        public void Generate_TenItems()
        {
            var fs = new FibonacciSequence<int>(10, 1, (x, y) => x + y);
            fs.ToList().Verify().SequenceEqual(new[] { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55 });
        }
    }
}
