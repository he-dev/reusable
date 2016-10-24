using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.FluentValidation.Testing;
using Reusable.FluentValidation.Validations;

namespace Reusable.Extensions.Tests
{
    [TestClass]
    public class TypeExtensionsTest
    {
        
    }

    [TestClass]
    public class NestedTypes
    {
        [TestMethod]
        public void GetTypesWithRoot()
        {
            var types = typeof(Foo).NestedTypes().ToList();
            types.Count.Verify(nameof(types)).IsEqual(7);
        }

        [TestMethod]
        public void GetTypesWithoutRoot()
        {
            var types = typeof(Foo).NestedTypes(t => t != typeof(Foo.SubFoo2)).ToList();
            types.Count.Verify(nameof(types)).IsEqual(2);
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
