using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Testing;
using Reusable.Validations;

namespace Reusable.Collections.Tests
{
    [TestClass]
    public class HarmonicSequenceTest
    {
        [TestMethod]
        public void Generate_SixItems()
        {
            var hs = new HarmonicSequence<int>(
                new LinearSequence<int>(count: 4, first: 1, constant: 1, add: (x, y) => x + y), 
                first: 12, 
                divide: (x, y) => x / y);
            hs.ToList().Verify().SequenceEqual(new[] { 12, 6, 4, 3 });
        }
    }
}
