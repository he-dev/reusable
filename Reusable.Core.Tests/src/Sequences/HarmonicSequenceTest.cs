using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse.Testing;
using Reusable.Fuse;
using Reusable.Sequences;

namespace Reusable.Collections.Tests
{
    [TestClass]
    public class HarmonicSequenceTest
    {
        [TestMethod]
        public void Generate_SixItems()
        {
            var hs = new HarmonicSequence<int>(
                new LinearSequence<int>(first: 1, constant: 1, add: (x, y) => x + y), 
                first: 12, 
                divide: (x, y) => x / y).Take(4);
            hs.ToList().Verify().SequenceEqual(new[] { 12, 6, 4, 3 });
        }
    }
}
