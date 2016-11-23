using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Converters;
using Reusable.Converters.Converters;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.TypeConversion.Tests
{
    [TestClass]
    public class SByteConvertersTest : ConverterTest
    {
        [TestMethod]
        public void ConvertStringToSByte()
        {
            Convert<StringToSByteConverter>(sbyte.MaxValue.ToString(), typeof(sbyte))
                .Verify()
                .IsNotNull()
                .IsTrue(x => (sbyte)x == sbyte.MaxValue);
        }

        [TestMethod]
        public void ConvertSByteToString()
        {
            Convert<SByteToStringConverter>(sbyte.MaxValue, typeof(string))
                .Verify()
                .IsNotNull()
                .IsTrue(x => (string)x == sbyte.MaxValue.ToString());
        }
    }
}