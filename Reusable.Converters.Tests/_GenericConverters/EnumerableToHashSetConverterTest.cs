using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Utilities.MSTest;

namespace Reusable.Converters.Tests
{
    [TestClass]
    public class EnumerableToHashSetConverterTest : ConverterTest
    {
        [TestMethod]
        public void Convert_StringArray_HashSetInt32()
        {
            var result =
                TypeConverter.Empty
                    .Add<StringToInt32Converter>()
                    .Add<EnumerableToHashSetConverter>()
                    .Convert(new[] { "3", "7" }, typeof(HashSet<int>)) as HashSet<int>;

            Assert.IsNotNull(result);
            Assert.That.Collection().CountEquals(2, result);
            Assert.IsTrue(result.Contains(3));
            Assert.IsTrue(result.Contains(7));
        }

        [TestMethod]
        public void Convert_Int32Array_HashSetString()
        {
            var result =
                TypeConverter.Empty
                    .Add<Int32ToStringConverter>()
                    .Add<EnumerableToHashSetConverter>()
                    .Convert(new[] { 3, 7 }, typeof(HashSet<string>)) as HashSet<string>;

            Assert.IsNotNull(result);
            Assert.That.Collection().CountEquals(2, result);
            Assert.IsTrue(result.Contains("3"));
            Assert.IsTrue(result.Contains("7"));
        }
    }
}