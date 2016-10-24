using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Converters;
using Reusable.FluentValidation.Testing;
using Reusable.FluentValidation.Validations;

namespace Reusable.TypeConversion.Tests
{
    [TestClass]
    public class HashSetConvertersTests : ConverterTest
    {
        [TestMethod]
        public void ConvertEnumerableStringToHashSetObject()
        {
            var result =
                TypeConverter.Empty
                    .Add<EnumerableObjectToHashSetObjectConverter>()
                    .Add<StringToInt32Converter>()
                    .Convert(new[] { "3", "7" }, typeof(HashSet<int>)) as HashSet<int>;

            result.Verify().IsNotNull();
            result.Count.Verify().IsEqual(2);
            result.Verify().Contains(3).Contains(7);
        }

        [TestMethod]
        public void ConvertEnumerableObjectToHashSetString()
        {
            var result =
                TypeConverter.Empty
                    .Add<EnumerableObjectToHashSetStringConverter>()
                    .Add<Int32ToStringConverter>()
                    .Convert(new[] { 3, 7 }, typeof(HashSet<string>)) as HashSet<string>;

            result.Verify().IsNotNull();
            result.Count.Verify().IsEqual(2);
            result.Verify().Contains("3").Contains("7");
        }
    }
}