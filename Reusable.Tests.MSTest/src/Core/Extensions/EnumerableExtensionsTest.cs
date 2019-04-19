using System.Linq;
using System.Linq.Custom;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests.Extensions
{
    [TestClass]
    public class enumerableTest
    {
        [TestMethod]
        public void Loop_EmptyCollection_NoneTakes()
        {
            var numbers = new int[0];
            Assert.IsFalse(numbers.Loop().Take(2).Any());
        }

        [TestMethod]
        public void Loop_LessThanAvailable_NoLoop()
        {
            var numbers = new[] { 1, 2, 3 };
            Assert.That.Collection().AreEqual(new[] { 1, 2 }, numbers.Loop().Take(2));
        }

        [TestMethod]
        public void Loop_MoreThanAvailable_OneLoop()
        {
            var numbers = new[] { 1, 2, 3 };
            Assert.That.Collection().AreEqual(new[] { 1, 2, 3, 1 }, numbers.Loop().Take(4));
        }

        [TestMethod]
        public void Loop_TwiceTheAvailable_TwoLoops()
        {
            var numbers = new[] { 1, 2, 3 };
            Assert.That.Collection().AreEqual(new[] { 1, 2, 3, 1, 2, 3, 1, 2 }, numbers.Loop().Take(8));
        }

        [TestMethod]
        public void Loop_MoreThanAvailableWithStartAt_OneLoop()
        {
            var numbers = new[] { 1, 2, 3 };
            Assert.That.Collection().AreEqual(new[] { 2, 3, 1, 2, 3, 1 }, numbers.Loop(1).Take(6));
        }

    }
}
