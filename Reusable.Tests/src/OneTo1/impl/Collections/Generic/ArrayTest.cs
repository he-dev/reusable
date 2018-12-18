using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters;
using Reusable.OneTo1.Converters.Collections;

namespace Reusable.Tests.OneTo1
{
    [TestClass]
    public class ArrayTest
    {
        [TestMethod]
        public void Convert_StringArray_Int32Array()
        {
            var converter = new CompositeConverter
            {
                typeof(EnumerableToArrayConverter),
                typeof(StringToInt32Converter),
            };

            var result = converter.Convert(new[] { "3", "7" }, typeof(int[])) as int[];

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