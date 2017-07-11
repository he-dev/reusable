using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.TestTools.UnitTesting.AssertExtensions.TehCodez;

namespace Reusable.TestTools.UnitTesting.Infrastructure
{
    //[TestClass]
    public abstract class ComparableTest<T>
    {
        [TestMethod]
        public void CompareTo_ThisIsLessThenOther_LessThenZero()
        {
            foreach (var t in GetObjectsGreaterThenThis())
            {
                Assert.That.IsLessThen(0, t.This.CompareTo(t.Other));
            }
        }

        [TestMethod]
        public void CompareTo_ThisIsEqualToOther_Zero()
        {
            foreach (var t in GetEqualObjects())
            {
                Assert.AreEqual(0, t.This.CompareTo(t.Other));
            }
        }

        [TestMethod]
        public void CompareTo_ThisIsGreaterThenOther_GreaterThenZero()
        {
            foreach (var t in GetObjectLessThenThis())
            {
                Assert.That.IsGreaterThen(0, t.This.CompareTo(t.Other));
            }
        }

        protected abstract IEnumerable<(IComparable<T> This, T Other)> GetObjectsGreaterThenThis();

        protected abstract IEnumerable<(IComparable<T> This, T Other)> GetEqualObjects();

        protected abstract IEnumerable<(IComparable<T> This, T Other)> GetObjectLessThenThis();
    }
}