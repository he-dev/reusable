using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.TypeConversion.Tests
{
    [TestClass]
    public class EnumerableToArrayConverterTest
    {
        [TestMethod]
        public void Convert_StringArray_Int32Array()
        {
            var result =
                TypeConverter.Empty
                    .Add<StringToInt32Converter>()
                    .Add<EnumerableToArrayConverter>()
                    .Convert(new[] { "3", "7" }, typeof(int[])) as int[];

            result.Verify().IsNotNull();
            result.Length.Verify().IsEqual(2);
            result[0].Verify().IsEqual(3);
            result[1].Verify().IsEqual(7);
        }

        [TestMethod]
        public void Convert_Int32Array_StringArray()
        {
            var result =
                TypeConverter.Empty
                    .Add<Int32ToStringConverter>()
                    .Add<EnumerableToArrayConverter>()
                    .Convert(new[] { 3, 7 }, typeof(string[])) as string[];

            result.Verify().IsNotNull();
            result.Length.Verify().IsEqual(2);
            result[0].Verify().IsEqual("3");
            result[1].Verify().IsEqual("7");
        }
    }
}