using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Testing;
using Reusable.Validations;

namespace Reusable.Extensions.Tests
{
    [TestClass]
    public class ObjectExtensionsTest
    {
        [TestMethod]
        public void GetElemenType_Array()
        {
            (new int[0]).GetElemenType().Verify().IsNotNull().IsTrue(x => x == typeof(int));
        }

        [TestMethod]
        public void GetElemenType_List()
        {
            (new List<int>()).GetElemenType().Verify().IsNotNull().IsTrue(x => x == typeof(int));
        }

        [TestMethod]
        public void GetElemenType_HashSet()
        {
            (new HashSet<int>()).GetElemenType().Verify().IsNotNull().IsTrue(x => x == typeof(int));
        }

        [TestMethod]
        public void GetElemenType_Dictionary()
        {
            (new Dictionary<string, int>()).GetElemenType().Verify().IsNotNull().IsTrue(x => x == typeof(int));
        }

        [TestMethod]
        public void GetElemenType_object()
        {
            (new object()).GetElemenType().Verify().IsNull();
        }
    }
}
