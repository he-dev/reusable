using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.TestTools.UnitTesting.Infrastructure.Collections
{
    public abstract class EqualityComparerTest<T>
    {
        private readonly IEqualityComparer<T> _comparer;

        protected EqualityComparerTest(IEqualityComparer<T> comparer)
        {
            _comparer = comparer;
        }

        [TestMethod]
        public void GetHashCode_SameElements_SameHashCodes()
        {
            foreach (var x in GetEqualElements())
            {
                Assert.AreEqual(_comparer.GetHashCode(x.Left), _comparer.GetHashCode(x.Right), $"{_comparer.GetHashCode(x.Left)} == {_comparer.GetHashCode(x.Right)}");
            }
        }

        [TestMethod]
        public void Equals_SameElements_True()
        {
            foreach (var x in GetEqualElements())
            {
                Assert.IsTrue(_comparer.Equals(x.Left, x.Right), $"{x.Left} == {x.Right}");
                Assert.IsTrue(_comparer.Equals(x.Right, x.Left), $"{x.Right} == {x.Left}");
                Assert.IsTrue(_comparer.Equals(x.Left, x.Left), $"{x.Left} == {x.Left}");
                Assert.IsTrue(_comparer.Equals(x.Right, x.Right), $"{x.Right} == {x.Right}");
            }
        }

        [TestMethod]
        public void Equals_DifferentElements_False()
        {
            foreach (var x in GetNonEqualElements())
            {
                Assert.IsFalse(_comparer.Equals(x.Left, x.Right), $"{x.Left} != {x.Right}");
                Assert.IsFalse(_comparer.Equals(x.Right, x.Left), $"{x.Right} != {x.Left}");
            }
        }

        protected abstract IEnumerable<(T Left, T Right)> GetEqualElements();

        protected abstract IEnumerable<(T Left, T Right)> GetNonEqualElements();
    }
}
