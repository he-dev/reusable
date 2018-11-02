using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Converters;
using Reusable.Convertia;
using Reusable.Convertia.Converters;
using Reusable.Convertia.Converters.Collections.Generic;

namespace Reusable.Tests.Converters
{
    [TestClass]
    public class ListTest
    {
        [TestMethod]
        public void Convert_StringArray_ListInt32()
        {
            var result =
                TypeConverter.Empty
                    .Add<StringToInt32Converter>()
                    .Add<EnumerableToListConverter>()
                    .Convert(new[] { "3", "7" }, typeof(List<int>)) as List<int>;

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(3, result[0]);
            Assert.AreEqual(7, result[1]);
        }

        [TestMethod]
        public void Convert_Int32Array_ListString()
        {
            var result =
                TypeConverter.Empty
                    .Add<Int32ToStringConverter>()
                    .Add<EnumerableToListConverter>()
                    .Convert(new[] { 3, 7 }, typeof(IList<string>)) as IList<string>;

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("3", result[0]);
            Assert.AreEqual("7", result[1]);
        }
    }
}