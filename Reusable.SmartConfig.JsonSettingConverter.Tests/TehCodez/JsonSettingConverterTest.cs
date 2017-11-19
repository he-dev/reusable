using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.SmartConfig.Tests
{
    [TestClass]
    public class JsonSettingConverterTest
    {
        [TestMethod]
        public void Deserialize_UnquotedEnumString_Enum()
        {
            var jsonSettingConverter = JsonSettingConverterFactory.CreateDefault();

            var result = jsonSettingConverter.Deserialize(@"bar", typeof(TestEnum));

            Assert.AreEqual(TestEnum.Bar, result);
        }

        [TestMethod]
        public void Deserialize_QuotedEnumString_Enum()
        {
            var jsonSettingConverter = JsonSettingConverterFactory.CreateDefault();

            var result = jsonSettingConverter.Deserialize(@"""bar""", typeof(TestEnum));

            Assert.AreEqual(TestEnum.Bar, result);
        }

        [TestMethod]
        public void Serialize_Enum_UnquotedEnum()
        {
            var jsonSettingConverter = JsonSettingConverterFactory.CreateDefault();

            var result = jsonSettingConverter.Serialize(TestEnum.Bar, new HashSet<Type>());

            Assert.AreEqual("Bar", result);
        }        

        private enum TestEnum
        {
            Foo,
            Bar,
            Baz
        }
    }
}
