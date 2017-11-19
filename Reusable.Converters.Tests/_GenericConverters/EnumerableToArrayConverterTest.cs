using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.Converters.Tests
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

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(3, result[0]);
            Assert.AreEqual(7, result[1]);
        }

        [TestMethod]
        public void Convert_Int32Array_StringArray()
        {
            var result =
                TypeConverter.Empty
                    .Add<Int32ToStringConverter>()
                    .Add<EnumerableToArrayConverter>()
                    .Convert(new[] { 3, 7 }, typeof(string[])) as string[];

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("3", result[0]);
            Assert.AreEqual("7", result[1]);
        }
    }
}