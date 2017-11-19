using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.Tests
{
    [TestClass]
    public class KeySetTest
    {
        [TestMethod]
        public void Equals_DifferentNames_False()
        {
            Assert.AreNotEqual(KeySet<SoftString>.Create("foo"), KeySet<SoftString>.Create("bar"));
        }

        [TestMethod]
        public void Equals_SameNames_True()
        {
            Assert.AreEqual(KeySet<SoftString>.Create("foo"), KeySet<SoftString>.Create("foo"));
        }

        [TestMethod]
        public void Equals_CommonNames_True()
        {
            Assert.AreEqual(KeySet<SoftString>.Create("foo"), KeySet<SoftString>.Create("bar", "FOO"));
        }       
    }    
}
