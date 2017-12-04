using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Collections;

namespace Reusable.Tests
{
    [TestClass]
    public class KeySetTest
    {
        [TestMethod]
        public void Equals_DifferentNames_False()
        {
            Assert.AreNotEqual(ImmutableKeySet<SoftString>.Create("foo"), ImmutableKeySet<SoftString>.Create("bar"));
        }

        [TestMethod]
        public void Equals_SameNames_True()
        {
            Assert.AreEqual(ImmutableKeySet<SoftString>.Create("foo"), ImmutableKeySet<SoftString>.Create("foo"));
        }

        [TestMethod]
        public void Equals_CommonNames_True()
        {
            Assert.AreEqual(ImmutableKeySet<SoftString>.Create("foo"), ImmutableKeySet<SoftString>.Create("bar", "FOO"));
        }       
    }    
}
