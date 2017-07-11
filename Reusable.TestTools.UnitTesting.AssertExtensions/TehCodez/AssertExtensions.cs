using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.TestTools.UnitTesting.AssertExtensions.TehCodez
{
    public static class AssertExtensions
    {
        public static void IsEmpty<T>(this Assert assert, IEnumerable<T> collection)
        {
            Assert.IsFalse(collection.Any());
        }

        public static void IsNotEmpty<T>(this Assert assert, IEnumerable<T> collection)
        {
            Assert.IsTrue(collection.Any());
        }

        public static void CountEquals<T>(this Assert assert, int count, IEnumerable<T> collection)
        {
            Assert.AreEqual(count, collection.Count());
        }

        public static void IsLessThen(this Assert assert, int then, int value)
        {
            Assert.IsTrue(value < then);
        }

        public static void IsGreaterThen(this Assert assert, int then, int value)
        {
            Assert.IsTrue(value > then);
        }
    }
}
