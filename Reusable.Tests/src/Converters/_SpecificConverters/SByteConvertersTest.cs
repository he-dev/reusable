using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.Converters.Tests
{
    [TestClass]
    public class SByteConvertersTest : ConverterTest
    {
        [TestMethod]
        public void Convert_String_SByte()
        {
            var result = Convert<StringToSByteConverter>(sbyte.MaxValue.ToString(), typeof(sbyte));

            Assert.IsTrue((sbyte)result == sbyte.MaxValue);
        }

        [TestMethod]
        public void Convert_SByte_String()
        {
            var result = Convert<SByteToStringConverter>(sbyte.MaxValue, typeof(string));

            Assert.IsTrue((string)result == sbyte.MaxValue.ToString());
        }
    }
}