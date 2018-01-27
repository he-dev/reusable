using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig.Utilities.Reflection;

namespace Reusable.SmartConfig.Core.Utilities.Tests.Reflection
{
    [TestClass]
    public class ClassFinderTest
    {
        [TestMethod]
        public void FindClass_InstanceClosure_TypeInstance()
        {
            var testClass1 = new TestClass1();
            var testClass2 = new TestClass2();

            var expression1 = (Expression<Func<object>>)(() => testClass1.Foo);
            var expression2 = (Expression<Func<object>>)(() => testClass2.Foo);

            var (type, instance) = ClassFinder.FindClass(expression2);

            Assert.AreEqual(typeof(TestClass2), type);
            Assert.AreSame(testClass2, instance);
        }

        [TestMethod]
        public void FindClass_InstanceLocal_TypeInstance()
        {
            var testClass1 = new TestClass1();
            var testClass2 = new TestClass2();

            testClass1.AssertFoo();
        }

        [TestMethod]
        public void FindClass_StaticClosure_TypeInstance()
        {
            var expression1 = (Expression<Func<object>>)(() => TestClass1.Bar);
            var expression2 = (Expression<Func<object>>)(() => TestClass2.Bar);

            var (type, instance) = ClassFinder.FindClass(expression2);

            Assert.AreEqual(typeof(TestClass2), type);
            Assert.IsNull(instance);
        }

        [TestMethod]
        public void FindClass_StaticLocal_TypeInstance()
        {
            TestClass1.AssertBar();
        }

        internal class TestClass1
        {
            public string Foo { get; set; }

            public static string Bar { get; set; }

            public void AssertFoo()
            {
                var expression1 = (Expression<Func<object>>)(() => Foo);
                var expression2 = (Expression<Func<object>>)(() => Foo);

                var (type, instance) = ClassFinder.FindClass(expression2);

                Assert.AreEqual(typeof(TestClass1), type);
                Assert.AreSame(this, instance);
            }

            public static void AssertBar()
            {
                var expression1 = (Expression<Func<object>>)(() => Bar);
                var expression2 = (Expression<Func<object>>)(() => Bar);

                var (type, instance) = ClassFinder.FindClass(expression2);

                Assert.AreEqual(typeof(TestClass1), type);
                Assert.IsNull(instance);
            }
        }

        internal class TestClass2
        {
            public string Foo { get; set; }

            public static string Bar { get; set; }
        }
    }

}
