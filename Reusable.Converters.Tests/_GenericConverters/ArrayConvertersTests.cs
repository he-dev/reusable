using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Converters;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.TypeConversion.Tests
{
    [TestClass]
    public class ArrayConvertersTests : ConverterTest
    {
        [TestMethod]
        public void ConvertEnumerableStringToArrayObject()
        {
            var result =
                TypeConverter.Empty
                    .Add<EnumerableObjectToArrayObjectConverter>()
                    .Add<StringToInt32Converter>()
                    .Convert(new[] { "3", "7" }, typeof(int[])) as int[];

            result.Verify().IsNotNull();
            result.Length.Verify().IsEqual(2);
            result[0].Verify().IsEqual(3);
            result[1].Verify().IsEqual(7);
        }

        [TestMethod]
        public void ConvertEnumerableObjectToArrayString()
        {
            var result =
                TypeConverter.Empty
                    .Add<EnumerableObjectToArrayStringConverter>()
                    .Add<Int32ToStringConverter>()
                    .Convert(new[] { 3, 7 }, typeof(string[])) as string[];

            result.Verify().IsNotNull();
            result.Length.Verify().IsEqual(2);
            result[0].Verify().IsEqual("3");
            result[1].Verify().IsEqual("7");
        }
    }
}