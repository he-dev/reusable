using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig.Utilities.Reflection;

namespace Reusable.Tests.SmartConfig.Utilities.Reflection
{
    [TestClass]
    public class ClassFinderTest
    {
        [TestMethod]
        public void FindClass_InstanceClosure_TypeInstance()
        {
            var testClass1 = new MockType1();
            var testClass2 = new MockType2();
            var testClass3 = new MockType2();

            var expression1 = (Expression<Func<object>>)(() => testClass1.Foo);
            var expression2 = (Expression<Func<object>>)(() => testClass2.Foo);

            var (type, instance) = ClassFinder.FindClass(expression2);

            Assert.AreEqual(typeof(MockType2), type);
            Assert.AreSame(testClass2, instance);
        }

        [TestMethod]
        public void FindClass_InstanceLocal_TypeInstance()
        {
            var testClass1 = new MockType1();
            var testClass2 = new MockType2();

            testClass1.AssertFoo();
        }

        [TestMethod]
        public void FindClass_StaticClosure_TypeInstance()
        {
            var expression1 = (Expression<Func<object>>)(() => MockType1.Bar);
            var expression2 = (Expression<Func<object>>)(() => MockType2.Bar);

            var (type, instance) = ClassFinder.FindClass(expression2);

            Assert.AreEqual(typeof(MockType2), type);
            Assert.IsNull(instance);
        }

        [TestMethod]
        public void FindClass_StaticLocal_TypeInstance()
        {
            MockType1.AssertBar();
        }

        [TestMethod]
        public void FindClass_Variable_TypeInstnce()
        {
            var foo = "fooo";
            var bar = new { bar = "baar" };

            var expression1 = (Expression<Func<object>>)(() => foo);
            var expression2 = (Expression<Func<object>>)(() => bar);

            var (type, instance) = ClassFinder.FindClass(expression1);
            var (type2, instance2) = ClassFinder.FindClass(expression2);

            Assert.AreEqual(typeof(string), type);
            Assert.AreSame(foo, instance);
        }

        [TestMethod]
        public void FindClass_BaseDerived_Base()
        {
            var foo1 = new DerivedClass1 { Foo = "bar" };
            var foo2 = new DerivedClass2 { Foo = "baz" };

            var expression1 = (Expression<Func<object>>)(() => foo1.Foo);
            var expression2 = (Expression<Func<object>>)(() => foo2.Foo);

            var (type, instance) = ClassFinder.FindClass(expression2);

            Assert.AreEqual(typeof(DerivedClass2), type);
            Assert.AreSame(foo2, instance);
        }

        [TestMethod]
        public void FindClass_BaseDerivedConstructor_Base()
        {
            new DerivedClass2("foo");
        }

        internal class MockType1
        {
            public string Foo { get; set; }

            public static string Bar { get; set; }

            public void AssertFoo()
            {
                var expression1 = (Expression<Func<object>>)(() => Foo);
                var expression2 = (Expression<Func<object>>)(() => Foo);

                var (type, instance) = ClassFinder.FindClass(expression2);

                Assert.AreEqual(typeof(MockType1), type);
                Assert.AreSame(this, instance);
            }

            public static void AssertBar()
            {
                var expression1 = (Expression<Func<object>>)(() => Bar);
                var expression2 = (Expression<Func<object>>)(() => Bar);

                var (type, instance) = ClassFinder.FindClass(expression2);

                Assert.AreEqual(typeof(MockType1), type);
                Assert.IsNull(instance);
            }
        }

        internal class MockType2
        {
            public string Foo { get; set; }

            public static string Bar { get; set; }
        }

        internal abstract class BaseClass
        {
            public string Foo { get; set; }
        }

        internal class DerivedClass1 : BaseClass
        {
            public DerivedClass1()
            {
                
            }

            public DerivedClass1(string str)
            {
                var expression1 = (Expression<Func<object>>)(() => this.Foo);

                var (type, instance) = ClassFinder.FindClass(expression1);

                Assert.AreEqual(typeof(DerivedClass1), type);
                Assert.AreSame(this, instance);
                
            }
        }

        internal class DerivedClass2 : BaseClass
        {
            public DerivedClass2()
            {

            }

            public DerivedClass2(string str)
            {
                var expression1 = (Expression<Func<object>>)(() => this.Foo);

                var (type, instance) = ClassFinder.FindClass(expression1);

                Assert.AreEqual(typeof(DerivedClass2), type);
                Assert.AreSame(this, instance);

            }
        }
    }

    [TestClass]
    public class SettingNameFactoryTest
    {
        private static readonly string Namespace = typeof(SettingNameFactoryTest).Namespace;

        [TestMethod]
        public void FindClass_InstanceClosure_TypeInstance()
        {
            var testClass1 = new MockType1();
            var testClass2 = new MockType2();
            var testClass3 = new MockType2();

            var expression1 = (Expression<Func<object>>)(() => testClass1.Foo);
            var expression2 = (Expression<Func<object>>)(() => testClass2.Foo);

            var settingNme = SettingNameFactory.CreateSettingName(expression2);

            Assert.AreEqual($"{Namespace}+{nameof(MockType2)}.Foo", settingNme.ToString());
            //Assert.AreSame(testClass2, instance);
        }

        [TestMethod]
        public void FindClass_InstanceLocal_TypeInstance()
        {
            var testClass1 = new MockType1();
            var testClass2 = new MockType2();

            testClass1.AssertFoo();
        }

        [TestMethod]
        public void FindClass_StaticClosure_TypeInstance()
        {
            var expression1 = (Expression<Func<object>>)(() => MockType1.Bar);
            var expression2 = (Expression<Func<object>>)(() => MockType2.Bar);

            var (type, instance) = ClassFinder.FindClass(expression2);

            Assert.AreEqual(typeof(MockType2), type);
            Assert.IsNull(instance);
        }

        [TestMethod]
        public void FindClass_StaticLocal_TypeInstance()
        {
            MockType1.AssertBar();
        }

        [TestMethod]
        public void FindClass_Variable_TypeInstnce()
        {
            var foo = "fooo";
            var bar = new { bar = "baar" };

            var expression1 = (Expression<Func<object>>)(() => foo);
            var expression2 = (Expression<Func<object>>)(() => bar);

            var (type, instance) = ClassFinder.FindClass(expression1);
            var (type2, instance2) = ClassFinder.FindClass(expression2);

            Assert.AreEqual(typeof(string), type);
            Assert.AreSame(foo, instance);
        }

        [TestMethod]
        public void FindClass_BaseDerived_Base()
        {
            var foo1 = new DerivedClass1 { Foo = "bar" };
            var foo2 = new DerivedClass2 { Foo = "baz" };

            var expression1 = (Expression<Func<object>>)(() => foo1.Foo);
            var expression2 = (Expression<Func<object>>)(() => foo2.Foo);

            var (type, instance) = ClassFinder.FindClass(expression2);

            Assert.AreEqual(typeof(DerivedClass2), type);
            Assert.AreSame(foo2, instance);
        }

        [TestMethod]
        public void FindClass_BaseDerivedConstructor_Base()
        {
            new DerivedClass2("foo");
        }

        internal class MockType1
        {
            public string Foo { get; set; }

            public static string Bar { get; set; }

            public void AssertFoo()
            {
                var expression1 = (Expression<Func<object>>)(() => Foo);
                var expression2 = (Expression<Func<object>>)(() => Foo);

                var (type, instance) = ClassFinder.FindClass(expression2);

                Assert.AreEqual(typeof(MockType1), type);
                Assert.AreSame(this, instance);
            }

            public static void AssertBar()
            {
                var expression1 = (Expression<Func<object>>)(() => Bar);
                var expression2 = (Expression<Func<object>>)(() => Bar);

                var (type, instance) = ClassFinder.FindClass(expression2);

                Assert.AreEqual(typeof(MockType1), type);
                Assert.IsNull(instance);
            }
        }

        internal class MockType2
        {
            public string Foo { get; set; }

            public static string Bar { get; set; }
        }

        internal abstract class BaseClass
        {
            public string Foo { get; set; }
        }

        internal class DerivedClass1 : BaseClass
        {
            public DerivedClass1()
            {

            }

            public DerivedClass1(string str)
            {
                var expression1 = (Expression<Func<object>>)(() => this.Foo);

                var (type, instance) = ClassFinder.FindClass(expression1);

                Assert.AreEqual(typeof(DerivedClass1), type);
                Assert.AreSame(this, instance);

            }
        }

        internal class DerivedClass2 : BaseClass
        {
            public DerivedClass2()
            {

            }

            public DerivedClass2(string str)
            {
                var expression1 = (Expression<Func<object>>)(() => this.Foo);

                var (type, instance) = ClassFinder.FindClass(expression1);

                Assert.AreEqual(typeof(DerivedClass2), type);
                Assert.AreSame(this, instance);

            }
        }
    }
}
