using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Collections;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests.Collections
{
    [TestClass]
    public class ComparerFactoryTest
    {
        [TestMethod]
        public void CanCompareSelectedValue()
        {
            var comparer = ComparerFactory<User>.Create(x => x.Age);

            Assert.That.Comparer().IsCanonical
            (
                value: new User(20),
                less: new User(15),
                equal: new User(20),
                greater: new User(30),
                comparer
            );

            Assert.IsTrue(comparer.Compare(default, new User(0)) < 0);
            Assert.IsTrue(comparer.Compare(new User(0), default) > 0);
            Assert.IsTrue(comparer.Compare(default, default) == 0);
        }

        [TestMethod]
        public void CanCompareNulls()
        {
            var comparer = ComparerFactory<User>.Create(x => x.Age);
            
            Assert.IsTrue(comparer.Compare(default, new User(0)) < 0);
            Assert.IsTrue(comparer.Compare(new User(0), default) > 0);
            Assert.IsTrue(comparer.Compare(default, default) == 0);
        }

        private class User
        {
            public User(int age) => Age = age;

            public int Age { get; }
        }
    }
}
