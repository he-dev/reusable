using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Converters;
using Reusable.Validations;

namespace Reusable.TypeConversion.Tests
{
    [TestClass]
    public class BooleanConvertersTests : ConverterTest
    {
        [TestMethod]
        public void ConvertStringToBoolean()
        {
            Convert<StringToBooleanConverter, bool>("true", typeof(bool)).IsTrue(x => x);
            Convert<StringToBooleanConverter, bool>("false", typeof(bool)).IsFalse(x => x);
        }

        [TestMethod]
        public void ConvertBooleanToString()
        {
            Convert<BooleanToStringConverter, string>(true, typeof(string)).IsEqual(bool.TrueString);
            Convert<BooleanToStringConverter, string>(false, typeof(string)).IsEqual(bool.FalseString);
        }
    }
}