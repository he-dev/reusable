using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.TestBase.Collections
{
    public abstract class EqualityComparerTest<T>
    {
        private readonly IEqualityComparer<T> _comparer;

        protected EqualityComparerTest(IEqualityComparer<T> comparer)
        {
            _comparer = comparer;
        }

        protected bool IgnoreHashCode { get; set; }

        [TestMethod]
        public void GetHashCode_SameElements_SameHashCodes()
        {
            if (IgnoreHashCode)
            {
                return;
            }

            foreach (var x in GetEqualElements())
            {
                Assert.AreEqual(_comparer.GetHashCode(x.Left), _comparer.GetHashCode(x.Right), $"{x.Left} == {x.Right}");
            }
        }

        [TestMethod]
        public void GetHashCode_DifferentElements_DifferentHashCodes()
        {
            if (IgnoreHashCode)
            {
                return;
            }

            foreach (var x in GetNonEqualElements())
            {
                Assert.AreNotEqual(_comparer.GetHashCode(x.Left), _comparer.GetHashCode(x.Right), $"{x.Left} != {x.Right}");
            }
        }

        [TestMethod]
        public void Equals_SameElements_True()
        {
            foreach (var x in GetEqualElements())
            {
                Assert.IsTrue(_comparer.Equals(x.Left, x.Right), $"{x.Left} == {x.Right}");
                Assert.IsTrue(_comparer.Equals(x.Left, x.Left), $"{x.Left} == {x.Right}");
                Assert.IsTrue(_comparer.Equals(x.Right, x.Right), $"{x.Left} == {x.Right}");
            }
        }

        [TestMethod]
        public void Equals_DifferentElements_False()
        {
            foreach (var x in GetNonEqualElements())
            {
                Assert.IsFalse(_comparer.Equals(x.Left, x.Right), $"{x.Left} != {x.Right}");
            }
        }

        protected abstract IEnumerable<(T Left, T Right)> GetEqualElements();

        protected abstract IEnumerable<(T Left, T Right)> GetNonEqualElements();
    }
}
