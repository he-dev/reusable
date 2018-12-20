using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.OneTo1;
using Reusable.OneTo1.Converters;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests.OneTo1
{
    [TestClass]
    public class TypeConverterTest
    {
        [TestMethod]
        public void EqualsIsCanonical()
        {
            Assert.That.Equatable().IsCanonical(new BooleanToStringConverter());
        }

        [TestMethod]
        public void DifferentConvertersAreNotEqual()
        {
            Assert.IsFalse(new BooleanToStringConverter().Equals(new Int32ToStringConverter()));
        }

        [TestMethod]
        public void SameConvertersAreEqual()
        {
            Assert.IsTrue(new BooleanToStringConverter().Equals(new BooleanToStringConverter()));
        }

        [TestMethod]
        public void DifferentConvertersHaveDifferentHashCodes()
        {
            Assert.AreNotEqual(new BooleanToStringConverter().GetHashCode(), new Int32ToStringConverter().GetHashCode());
        }

        [TestMethod]
        public void SameConverterHaveSameHashCodes()
        {
            Assert.AreEqual(new BooleanToStringConverter().GetHashCode(), new BooleanToStringConverter().GetHashCode());
        }               

        [TestMethod]
        public void ConversionBetweenSameTypesDoesNothing()
        {
            var converter = TypeConverter.Empty.Add<StringToBooleanConverter>();
            var result = converter.Convert("foo", typeof(string));
            Assert.IsTrue(result is string y && y == "foo");
        }
    }
}
