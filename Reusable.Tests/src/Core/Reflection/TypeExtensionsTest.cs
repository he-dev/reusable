using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Reflection;

namespace Reusable.Tests.Reflection
{
    [TestClass]
    public class TypeExtensionsTest
    {
        [TestMethod]
        public void IsEnumerable_CanIdentifyString()
        {
            Assert.IsTrue(typeof(string).IsEnumerableOfT());
        }
        
        [TestMethod]
        public void IsEnumerable_CanIdentifyListOfT()
        {
            Assert.IsTrue(typeof(List<string>).IsEnumerableOfT());
        }
        
        [TestMethod]
        public void IsEnumerable_CanIdentifyIListOfT()
        {
            Assert.IsTrue( typeof(IList<string>).IsEnumerableOfT());
        }
        
        [TestMethod]
        public void IsEnumerable_CanIdentifyIEnumerableOfT()
        {
            Assert.IsTrue( typeof(IEnumerable<string>).IsEnumerableOfT());
        }
        
        [TestMethod]
        public void IsEnumerable_RejectsNotEnumerableTypes()
        {
            Assert.IsFalse(typeof(int).IsEnumerableOfT());
            Assert.IsFalse(typeof(DateTime).IsEnumerableOfT());
        }
        
        [TestMethod]
        public void IsEnumerable_RejectsIgnoredTypes()
        {
            Assert.IsFalse(typeof(IList<string>).IsEnumerableOfT(except: typeof(IList<string>)));
            Assert.IsFalse(typeof(string).IsEnumerableOfT(except: typeof(string)));
        }
    }
}