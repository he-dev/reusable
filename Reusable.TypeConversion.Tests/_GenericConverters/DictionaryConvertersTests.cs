using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Converters;
using Reusable.Testing;
using Reusable.Validations;

namespace Reusable.TypeConversion.Tests
{
    [TestClass]
    public class DictionaryConvertersTests : ConverterTest
    {
        [TestMethod]
        public void ConvertDictionaryStringStringToDictionaryObjectObject()
        {
            var result =
                TypeConverter.Empty
                    .Add<DictionaryObjectObjectToDictionaryObjectObjectConverter>()
                    .Add<StringToInt32Converter>()
                    .Add<StringToEnumConverter>()
                    .Convert(new Dictionary<string, string>
                    {
                        ["Foo"] = "3",
                        ["Bar"] = "7"
                    }, typeof(Dictionary<TestEnum, int>)) as Dictionary<TestEnum, int>;

            result.Verify().IsNotNull();
            result.Count.Verify().IsEqual(2);
            result[TestEnum.Foo].Verify().IsEqual(3);
            result[TestEnum.Bar].Verify().IsEqual(7);
        }

        [TestMethod]
        public void ConvertDictionaryObjectObjectToDictionaryStringString()
        {
            var result =
                TypeConverter.Empty
                    .Add<DictionaryObjectObjectToDictionaryStringStringConverter>()
                    .Add<Int32ToStringConverter>()
                    .Add<EnumToStringConverter>()
                    .Convert(new Dictionary<TestEnum, int>
                    {
                        [TestEnum.Foo] = 3,
                        [TestEnum.Bar] = 7
                    }, typeof(Dictionary<string, string>)) as Dictionary<string, string>;

            result.Verify().IsNotNull();
            result.Count.Verify().IsEqual(2);
            result["Foo"].Verify().IsEqual("3");
            result["Bar"].Verify().IsEqual("7");
        }

        private enum TestEnum
        {
            Foo,
            Bar
        }
    }
}