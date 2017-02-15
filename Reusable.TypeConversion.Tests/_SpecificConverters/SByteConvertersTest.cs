using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.TypeConversion.Tests
{
    [TestClass]
    public class SByteConvertersTest : ConverterTest
    {
        [TestMethod]
        public void Convert_String_SByte()
        {
            Convert<StringToSByteConverter>(sbyte.MaxValue.ToString(), typeof(sbyte))
                .Verify()
                .IsNotNull()
                .IsTrue(x => (sbyte)x == sbyte.MaxValue);
        }

        [TestMethod]
        public void Convert_SByte_String()
        {
            Convert<SByteToStringConverter>(sbyte.MaxValue, typeof(string))
                .Verify()
                .IsNotNull()
                .IsTrue(x => (string)x == sbyte.MaxValue.ToString());
        }
    }
}