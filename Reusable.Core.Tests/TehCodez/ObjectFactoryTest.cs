using System;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.Tests
{
    [TestClass]
    public class ObjectFactoryTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var person = ObjectFactory.CreateInstance<IPerson>();

            Assert.ThrowsException<InvalidOperationException>(() => person.FirstName);
            Assert.ThrowsException<ArgumentNullException>(() => person.FirstName == null);
        }

        public interface IPerson
        {
            [NotNull]
            string FirstName { get; set; }

            [NotNull]
            string LastName { get; set; }

            string NickName { get; set; }
        }
    }
}
