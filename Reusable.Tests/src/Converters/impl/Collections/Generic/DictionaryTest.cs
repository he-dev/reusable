using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Converters;
using Reusable.Convertia;
using Reusable.Convertia.Converters;
using Reusable.Convertia.Converters.Collections.Generic;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests.Converters
{
    [TestClass]
    public class DictionaryTest
    {
        [TestMethod]
        public void Convert_DictionaryStringString_DictionaryEnumInt32()
        {
            var result =
                TypeConverter.Empty
                    .Add<StringToInt32Converter>()
                    .Add<StringToEnumConverter>()
                    .Add<DictionaryToDictionaryConverter>()
                    .Convert(new Dictionary<string, string>
                    {
                        ["Foo"] = "3",
                        ["Bar"] = "7"
                    }, typeof(Dictionary<TestEnum, int>)) as Dictionary<TestEnum, int>;

            Assert.IsNotNull(result);
            Assert.That.Collection().CountEquals(2, result);
            Assert.AreEqual(3, result[TestEnum.Foo]);
            Assert.AreEqual(7, result[TestEnum.Bar]);
        }

        [TestMethod]
        public void Convert_DictionaryEnumInt32_DictionaryStringString()
        {
            var converter =
                TypeConverter.Empty
                    .Add<Int32ToStringConverter>()
                    .Add<EnumToStringConverter>()
                    .Add<DictionaryToDictionaryConverter>();

            var result = converter.Convert(new Dictionary<TestEnum, int>
            {
                [TestEnum.Foo] = 3,
                [TestEnum.Bar] = 7
            }, typeof(Dictionary<string, string>)) as Dictionary<string, string>;

            Assert.IsNotNull(result);
            Assert.That.Collection().CountEquals(2, result);
            Assert.AreEqual("3",  result["Foo"]);
            Assert.AreEqual("7", result["Bar"]);
        }

        private enum TestEnum
        {
            Foo,
            Bar
        }
    }
}