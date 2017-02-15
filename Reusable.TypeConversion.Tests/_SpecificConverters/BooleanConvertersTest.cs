using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.TypeConversion.Tests
{
    [TestClass]
    public class BooleanConvertersTest
    {
        [TestMethod]
        public void Convert_String_Boolean()
        {
            var converter = TypeConverter.Empty.Add<StringToBooleanConverter>();
            var result = converter.Convert("true", typeof(bool));
            result.Verify().IsNotNull().IsTrue(x => (bool)x);
        }

        [TestMethod]
        public void Convert_Boolean_String()
        {
            var converter = TypeConverter.Empty.Add<BooleanToStringConverter>();
            var result = converter.Convert(true, typeof(string));
            result.Verify().IsNotNull().IsTrue(x => (string)x == bool.TrueString);
        }

        [TestMethod]
        public void Convert_Boolean_Boolean()
        {
            var converter = TypeConverter.Empty.Add<StringToBooleanConverter>();
            var result = converter.Convert(true, typeof(bool));
            result.Verify().IsNotNull().IsTrue(x => (bool)x);
        }
    }
}