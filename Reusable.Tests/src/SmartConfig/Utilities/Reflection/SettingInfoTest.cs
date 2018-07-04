using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig.Utilities.Reflection;

namespace Reusable.Tests.SmartConfig.Utilities.Reflection
{
    [TestClass]
    public class SettingInfoTest
    {
        private static readonly string Namespace = typeof(SettingInfoTest).Namespace;

        [TestMethod]
        public void GetSettingInfo_Instance_SettingInfo()
        {
            var mock1 = new MockClass1();
            var mock2 = new MockClass2();
            var mock3 = new MockClass2();

            var expression1 = (Expression<Func<object>>)(() => mock1.InstanceProperty);
            var expression2 = (Expression<Func<object>>)(() => mock2.InstanceProperty);

            var setting = SettingInfo.FromExpression(expression2, false, null);

            Assert.AreEqual($"{Namespace}+MockClass2.InstanceProperty", setting.ToString());
        }

        [TestMethod]
        public void CreateSettingInfo_InstanceClosure_SettingInfo()
        {
            var mock1 = new MockClass1();
            var mock2 = new MockClass2();

            Assert.AreEqual(
                $"{Namespace}+MockClass1.InstanceProperty",
                mock1.GetSettingInfo_InstanceClosure().ToString());
        }

        [TestMethod]
        public void CreateSettingInfo_Static_SettingInfo()
        {
            var expression1 = (Expression<Func<object>>)(() => MockClass1.StaticProperty);
            var expression2 = (Expression<Func<object>>)(() => MockClass2.StaticProperty);

            Assert.AreEqual(
                $"{Namespace}+MockClass2.StaticProperty",
                SettingInfo.FromExpression(expression2, false, null).ToString());
        }

        [TestMethod]
        public void CreateSettingInfo_StaticClosure_SettingInfo()
        {
            Assert.AreEqual(
                $"{Namespace}+MockClass1.StaticProperty",
                MockClass1.GetSettingInfo_StaticClosure().ToString());
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
            var mock1 = new MockDerivedClass1 { InstanceProperty = "mock1" };
            var mock2 = new MockDerivedClass2 { InstanceProperty = "mock2" };

            var expression1 = (Expression<Func<object>>)(() => mock1.InstanceProperty);
            var expression2 = (Expression<Func<object>>)(() => mock2.InstanceProperty);

            Assert.AreEqual(
                $"{Namespace}+MockDerivedClass2.InstanceProperty",
                SettingInfo.FromExpression(expression2, false, null).ToString());
        }

        //[TestMethod]
        //public void FindClass_BaseDerivedConstructor_Base()
        //{
        //    new MockDerivedClass2("foo");
        //}

        internal class MockClass1
        {
            public string InstanceProperty { get; set; }

            public SettingInfo GetSettingInfo_InstanceClosure()
            {
                var expression1 = (Expression<Func<string>>)(() => InstanceProperty);
                var expression2 = (Expression<Func<string>>)(() => InstanceProperty);

                return SettingInfo.FromExpression(expression2, false, null);
            }

            public static string StaticProperty { get; set; }

            public static SettingInfo GetSettingInfo_StaticClosure()
            {
                var expression1 = (Expression<Func<string>>)(() => StaticProperty);
                var expression2 = (Expression<Func<string>>)(() => StaticProperty);

                return SettingInfo.FromExpression(expression2, false, null);
            }
        }

        internal class MockClass2
        {
            public string InstanceProperty { get; set; }

            public static string StaticProperty { get; set; }
        }

        internal abstract class MockBaseClass
        {
            public string InstanceProperty { get; set; }
        }

        internal class MockDerivedClass1 : MockBaseClass
        {
            public MockDerivedClass1() { }

            //public MockDerivedClass1(string str)
            //{
            //    var expression1 = (Expression<Func<object>>)(() => InstanceProperty);

            //    var (type, instance) = ClassFinder.FindClass(expression1);

            //    Assert.AreEqual(typeof(MockDerivedClass1), type);
            //    Assert.AreSame(this, instance);

            //}
        }

        internal class MockDerivedClass2 : MockBaseClass
        {
            public MockDerivedClass2()
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