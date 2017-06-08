using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.TypeConversion.Tests
{
    [TestClass]
    public class EnumerableToListConverterTest
    {
        [TestMethod]
        public void Convert_StringArray_ListInt32()
        {
            var result =
                TypeConverter.Empty
                    .Add<StringToInt32Converter>()
                    .Add<EnumerableToListConverter>()
                    .Convert(new[] { "3", "7" }, typeof(List<int>)) as List<int>;

            result.Verify().IsNotNull();
            result.Count.Verify().IsEqual(2);
            result[0].Verify().IsEqual(3);
            result[1].Verify().IsEqual(7);
        }

        [TestMethod]
        public void Convert_Int32Array_ListString()
        {
            var result =
                TypeConverter.Empty
                    .Add<Int32ToStringConverter>()
                    .Add<EnumerableToListConverter>()
                    .Convert(new[] { 3, 7 }, typeof(IList<string>)) as IList<string>;

            result.Verify().IsNotNull();
            result.Count.Verify().IsEqual(2);
            result[0].Verify().IsEqual("3");
            result[1].Verify().IsEqual("7");
        }
    }
}