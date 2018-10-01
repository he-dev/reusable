using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Reflection;

namespace Reusable.Tests.Reflection
{
    [TestClass]
    public class TypeExtensionsTest
    {
        [TestMethod]
        public void IsEnumerable_List_True()
        {
            var actualType = typeof(List<string>);
            Assert.IsTrue(actualType.IsEnumerableOfT(except: typeof(string)));
        }
        
        [TestMethod]
        public void IsEnumerable_IEnumerableOfString_True()
        {
            var actualType = typeof(IEnumerable<string>);
            Assert.IsTrue(actualType.IsEnumerableOfT(except: typeof(string)));
        }
    }
}