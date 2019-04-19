using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.OneTo1;
using Reusable.SmartConfig;

namespace Reusable.Tests.SmartConfig.Reflection
{
    [TestClass]
    public class SettingExpressionExtensionsTest
    {
        private static readonly ITypeConverter UriConverter = new UriStringToSettingIdentifierConverter();
        
        [TestMethod]
        public void GetSettingName_Instance_FullName()
        {
            var testClass1 = new TestClass1();
            var testClass2 = new TestClass2();
            var expression1 = ((Expression<Func<object>>)(() => testClass1.Foo));
            var expression2 = ((Expression<Func<object>>)(() => testClass2.Foo));
            var settingMetadata = SettingMetadata.FromExpression(expression2);
            var settingName = (string)UriConverter.Convert<SettingIdentifier>(SettingRequestFactory.CreateSettingRequest(settingMetadata, null));
            
            Assert.AreEqual("Reusable.Tests.SmartConfig.Reflection+TestClass2.Foo", settingName);
        }

        [TestMethod]
        public void GetSettingName_Local_FullName()
        {
            var testClass1 = new TestClass1();
            var testClass2 = new TestClass2();
            var expression1 = ((Expression<Func<object>>)(() => testClass1.Foo));
            var expression2 = ((Expression<Func<object>>)(() => testClass2.Foo));
            
            testClass1.AssertFoo();
        }
    }

    internal class TestClass1
    {       
        public string Foo { get; set; }

        public static string Bar { get; set; }

        public void AssertFoo()
        {
            var expression1 = ((Expression<Func<object>>)(() => Foo));
            var expression2 = ((Expression<Func<object>>)(() => Foo));

            var settingMetadata = SettingMetadata.FromExpression(expression2);
            //var settingName = (string)SettingRequestFactory.CreateSettingRequest(settingMetadata, null);
            
            //Assert.AreEqual("Reusable.Tests.SmartConfig.Reflection+TestClass1.Foo", settingName);
        }
    }

    internal class TestClass2
    {        
        public string Foo { get; set; }
    }
}
