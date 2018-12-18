using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters;

namespace Reusable.Tests.OneTo1
{
    [TestClass]
    public class BooleanTest
    {
        [TestMethod]
        public void Convert_String_Boolean()
        {
            var converter = TypeConverter.Empty.Add<StringToBooleanConverter>();
            var result = converter.Convert("true", typeof(bool));
            Assert.IsTrue((bool)result);
        }

        [TestMethod]
        public void Convert_Boolean_String()
        {
            var converter = TypeConverter.Empty.Add<BooleanToStringConverter>();
            var result = converter.Convert(true, typeof(string));
            Assert.IsTrue(((string)result) == bool.TrueString);
        }

        [TestMethod]
        public void Convert_Boolean_Boolean()
        {
            var converter = TypeConverter.Empty.Add<StringToBooleanConverter>();
            var result = converter.Convert(true, typeof(bool));
            Assert.IsTrue((bool)result);
        }
    }
}