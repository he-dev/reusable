using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig.Reflection;

namespace Reusable.Tests.SmartConfig.Reflection
{
    [TestClass]
    public class SettingInfoTest
    {
        private static readonly string Namespace = typeof(SettingInfoTest).Namespace;
        private static readonly string Assembly = typeof(SettingInfoTest).Assembly.GetName().Name;

        [TestMethod]
        public void GetSettingInfo_CanReflectInstanceProperty()
        {
            var test1 = new TestClass1();
            var test2 = new TestClass2();
            var test3 = new TestClass2();

            // We need multiple expressions to test whether it finds the correct closure.
            var expression1 = (Expression<Func<object>>)(() => test1.InstanceProperty);
            var expression2 = (Expression<Func<object>>)(() => test2.InstanceProperty);

            var setting = SettingMetadata.FromExpression(expression2, false);

            var asm = typeof(SettingInfoTest).Assembly.GetName().Name;
            Assert.AreEqual($"{Assembly}:{Namespace}+TestClass2.InstanceProperty", setting.ToString());
        }

        [TestMethod]
        public void CreateSettingInfo_InstanceClosure_SettingInfo()
        {
            var mock1 = new TestClass1();
            var mock2 = new TestClass2();

            Assert.AreEqual(
                $"{Namespace}+MockClass1.InstanceProperty",
                mock1.GetSettingMetadata_InstanceClosure().ToString());
        }

        [TestMethod]
        public void CreateSettingInfo_Static_SettingInfo()
        {
            var expression1 = (Expression<Func<object>>)(() => TestClass1.StaticProperty);
            var expression2 = (Expression<Func<object>>)(() => TestClass2.StaticProperty);

            Assert.AreEqual(
                $"{Namespace}+MockClass2.StaticProperty",
                SettingMetadata.FromExpression(expression2, false).ToString());
        }

        [TestMethod]
        public void CreateSettingInfo_StaticClosure_SettingInfo()
        {
            Assert.AreEqual(
                $"{Namespace}+MockClass1.StaticProperty",
                TestClass1.GetSettingMetadata_StaticClosure().ToString());
        }

        //[TestMethod]
        //public void CreateSettingInfo_Variable_SettingInfo()
        //{
        //    var foo = "fooo";
        //    var bar = new { bar = "baar" };

        //    var expression1 = (Expression<Func<object>>)(() => foo);
        //    var expression2 = (Expression<Func<object>>)(() => bar);

        //    //var (type, instance) = ClassFinder.FindClass(expression1);
        //    //var (type2, instance2) = ClassFinder.FindClass(expression2);

        //    //Assert.AreEqual(typeof(string), type);
        //    //Assert.AreSame(foo, instance);

        //    Assert.AreEqual(
        //        $"{Namespace}+MockClass2.InstanceProperty",
        //        SettingInfo.FromExpression(expression2, false, null).ToString());
        //}

        [TestMethod]
        public void CreateSettingInfo_DerivedInstance_SettingInfo()
        {
            var mock1 = new TestDerivedClass1 { InstanceProperty = "mock1" };
            var mock2 = new TestDerivedClass2 { InstanceProperty = "mock2" };

            var expression1 = (Expression<Func<object>>)(() => mock1.InstanceProperty);
            var expression2 = (Expression<Func<object>>)(() => mock2.InstanceProperty);

            Assert.AreEqual(
                $"{Namespace}+MockDerivedClass2.InstanceProperty",
                SettingMetadata.FromExpression(expression2, false).ToString());
        }

        //[TestMethod]
        //public void FindClass_BaseDerivedConstructor_Base()
        //{
        //    new MockDerivedClass2("foo");
        //}

        internal class TestClass1
        {
            public string InstanceProperty { get; set; }

            public SettingMetadata GetSettingMetadata_InstanceClosure()
            {
                var expression1 = (Expression<Func<string>>)(() => InstanceProperty);
                var expression2 = (Expression<Func<string>>)(() => InstanceProperty);

                return SettingMetadata.FromExpression(expression2, false);
            }

            public static string StaticProperty { get; set; }

            public static SettingMetadata GetSettingMetadata_StaticClosure()
            {
                var expression1 = (Expression<Func<string>>)(() => StaticProperty);
                var expression2 = (Expression<Func<string>>)(() => StaticProperty);

                return SettingMetadata.FromExpression(expression2, false);
            }
        }

        internal class TestClass2
        {
            public string InstanceProperty { get; set; }

            public static string StaticProperty { get; set; }
        }

        internal abstract class TestBaseClass
        {
            public string InstanceProperty { get; set; }
        }

        internal class TestDerivedClass1 : TestBaseClass
        {
            public TestDerivedClass1() { }

            //public MockDerivedClass1(string str)
            //{
            //    var expression1 = (Expression<Func<object>>)(() => InstanceProperty);

            //    var (type, instance) = ClassFinder.FindClass(expression1);

            //    Assert.AreEqual(typeof(MockDerivedClass1), type);
            //    Assert.AreSame(this, instance);

            //}
        }

        internal class TestDerivedClass2 : TestBaseClass
        {
            public TestDerivedClass2()
            {

            }

            //public MockDerivedClass2(string str)
            //{
            //    var expression1 = (Expression<Func<object>>)(() => InstanceProperty);

            //    var (type, instance) = ClassFinder.FindClass(expression1);

            //    Assert.AreEqual(typeof(MockDerivedClass2), type);
            //    Assert.AreSame(this, instance);

            //}
        }
    }
}