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
            Reflection.GetElemenType<int[]>().Verify().IsNotNull().IsTrue(x => x == typeof(int));
        }

        [TestMethod]
        public void GetElemenType_List()
        {
            Reflection.GetElemenType<List<int>>().Verify().IsNotNull().IsTrue(x => x == typeof(int));
        }

        [TestMethod]
        public void GetElemenType_HashSet()
        {
            Reflection.GetElemenType<HashSet<int>>().Verify().IsNotNull().IsTrue(x => x == typeof(int));
        }

        [TestMethod]
        public void GetElemenType_Dictionary()
        {
            Reflection.GetElemenType<Dictionary<string, int>>().Verify().IsNotNull().IsTrue(x => x == typeof(int));
        }

        [TestMethod]
        public void GetElemenType_object()
        {
            Reflection.GetElemenType<object>().Verify().IsNull();
        }
    }
}
