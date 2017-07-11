using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.TestTools.UnitTesting.Infrastructure
{
    //[TestClass]
    public abstract class EquatableTest<T>
    {
        [TestMethod]
        public void Equals_ThisIsEqualToOther_True()
        {
            foreach (var t in GetEqualObjects())
            {
                Assert.IsTrue(t.This.Equals(t.Other));
            }
        }

        [TestMethod]
        public void Equals_ThisIsNotEqualToOther_False()
        {
            foreach (var t in GetNotEqualObjects())
            {
                Assert.IsFalse(t.This.Equals(t.Other));
            }
        }

        protected abstract IEnumerable<(IEquatable<T> This, T Other)> GetEqualObjects();

        protected abstract IEnumerable<(IEquatable<T> This, T Other)> GetNotEqualObjects();
    }
}
