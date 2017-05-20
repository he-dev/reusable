using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse.Testing;
using Reusable.Fuse;

namespace Reusable.Tests
{
    [TestClass]
    public class ObjectExtensionsTest
    {
        [TestMethod]
        public void GetElemenType_Array()
        {
            Reflection.GetElementType<int[]>().Verify().IsNotNull().IsTrue(x => x == typeof(int));
        }

        [TestMethod]
        public void GetElemenType_List()
        {
            Reflection.GetElementType<List<int>>().Verify().IsNotNull().IsTrue(x => x == typeof(int));
        }

        [TestMethod]
        public void GetElemenType_HashSet()
        {
            Reflection.GetElementType<HashSet<int>>().Verify().IsNotNull().IsTrue(x => x == typeof(int));
        }

        [TestMethod]
        public void GetElemenType_Dictionary()
        {
            Reflection.GetElementType<Dictionary<string, int>>().Verify().IsNotNull().IsTrue(x => x == typeof(int));
        }

        [TestMethod]
        public void GetElemenType_object()
        {
            Reflection.GetElementType<object>().Verify().IsNull();
        }
    }
}
