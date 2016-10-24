using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Converters;
using Reusable.FluentValidation.Testing;
using Reusable.FluentValidation.Validations;

namespace Reusable.TypeConversion.Tests
{
    [TestClass]
    public class ListConvertersTests : ConverterTest
    {
        [TestMethod]
        public void ConvertEnumerableStringToListObject()
        {
            var result =
                TypeConverter.Empty
                    .Add<EnumerableObjectToListObjectConverter>()
                    .Add<StringToInt32Converter>()
                    .Convert(new[] { "3", "7" }, typeof(List<int>)) as List<int>;

            result.Verify().IsNotNull();
            result.Count.Verify().IsEqual(2);
            result[0].Verify().IsEqual(3);
            result[1].Verify().IsEqual(7);
        }

        [TestMethod]
        public void ConvertEnumerableObjectToListString()
        {
            var result =
                TypeConverter.Empty
                    .Add<EnumerableObjectToListStringConverter>()
                    .Add<Int32ToStringConverter>()
                    .Convert(new[] { 3, 7 }, typeof(IList<string>)) as IList<string>;

            result.Verify().IsNotNull();
            result.Count.Verify().IsEqual(2);
            result[0].Verify().IsEqual("3");
            result[1].Verify().IsEqual("7");
        }
    }
}