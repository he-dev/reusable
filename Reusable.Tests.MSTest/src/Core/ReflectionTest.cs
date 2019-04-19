using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Reflection;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests
{
    [TestClass]
    public class ReflectionTest
    {
        [TestMethod]
        public void Flatten_Unfiltered_AllTypes()
        {
            var types = typeof(Foo).SelectMany().ToList();
           Assert.That.Collection().CountEquals(7, types);
            //Assert.AreEqual(1, types.Single(x => x.Type == typeof(Foo.SubFoo2)).Depth);
            //Assert.AreEqual(3, types.Single(x => x.Type == typeof(Foo.SubFoo2.SubSubFoo1.SubSubSubFoo1)).Depth);
        }

        [TestMethod]
        public void Flatten_Filtered_FewerTypes()
        {
            var types = typeof(Foo).SelectMany().Where(t => t.Type != typeof(Foo.SubFoo2)).ToList();
            Assert.That.Collection().CountEquals(6, types);
        }

        private static class Foo
        {
            public static class SubFoo1 { }

            public static class SubFoo2
            {
                public static class SubSubFoo1
                {
                    public static class SubSubSubFoo1 { }
                }

                public static class SubSubFoo2 { }

                public static class Baz { }
            }
        }
    }
    
    [TestClass]
    public class IsStatic
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RequiresType()
        {
            ((Type)null).IsStatic();
        }

        [TestMethod]
        public void ReturnsTrueIfTypeIsStatic()
        {
            Assert.IsTrue(typeof(StaticTestClass).IsStatic());
        }

        [TestMethod]
        public void ReturnsFalseIfTypeNotStatic()
        {
            Assert.IsFalse(typeof(NonStaticTestClass).IsStatic());
        }

        public static class StaticTestClass { }

        public class NonStaticTestClass { }
    }
}
