using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters;
using Reusable.OneTo1.Converters.Collections.Generic;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests.OneTo1
{
    [TestClass]
    public class HashSetTest : ConverterTest
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

        //[TestMethod]
        //public void Convert_StringArray_HashSetInt32_2()
        //{
        //    var result =
        //        TypeConverter.Empty
        //            .Add<StringToInt32Converter>()
        //            .Add<EnumerableToHashSetConverter<int>>()
        //            .Convert(new[] { "3", "7" }, typeof(HashSet<int>)) as HashSet<int>;

        //    Assert.IsNotNull(result);
        //    Assert.That.Collection().CountEquals(2, result);
        //    Assert.IsTrue(result.Contains(3));
        //    Assert.IsTrue(result.Contains(7));
        //}

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