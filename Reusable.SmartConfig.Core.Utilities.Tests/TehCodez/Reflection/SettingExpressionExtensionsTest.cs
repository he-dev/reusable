using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig.Utilities.Reflection;

namespace Reusable.SmartConfig.Core.Utilities.Tests.Reflection
{
    [TestClass]
    public class SettingExpressionExtensionsTest
    {
        [TestMethod]
        public void GetSettingName_Closure_FullName()
        {
            var testClass1 = new TestClass1();
            var testClass2 = new TestClass2();
            var expression1 = ((Expression<Func<object>>)(() => testClass1.Foo));
            var expression2 = ((Expression<Func<object>>)(() => testClass2.Foo));
            var settingName2 = expression2.GetSettingName();
            Assert.AreEqual("Reusable.SmartConfig.Core.Utilities.Tests.Reflection+TestClass2.Foo", settingName2);
        }

        [TestMethod]
        public void GetSettingName_Instance_FullName()
        {
            var testClass1 = new TestClass1();
            var testClass2 = new TestClass2();
            var expression1 = ((Expression<Func<object>>)(() => testClass1.Foo));
            var expression2 = ((Expression<Func<object>>)(() => testClass2.Foo));
            
            testClass1.Assert();
        }
    }

    class TestClass1
    {       
        public string Foo { get; set; }

        public void Assert()
        {
            var expression1 = ((Expression<Func<object>>)(() => Foo));
            var expression2 = ((Expression<Func<object>>)(() => Foo));

            var settingName2 = expression2.GetSettingName();
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual("Reusable.SmartConfig.Core.Utilities.Tests.Reflection+TestClass1.Foo", settingName2);
        }
    }

    class TestClass2
    {
        public TestClass2()
        {
            
        }

        public TestClass2(IConfiguration configuration)
        {
            
        }

        public string Foo { get; set; }
    }
}
