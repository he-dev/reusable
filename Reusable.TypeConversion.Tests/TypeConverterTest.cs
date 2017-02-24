using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse.Testing;
using Reusable.Fuse;

namespace Reusable.TypeConversion.Tests
{
    [TestClass]
    public class TypeConverterTest
    {
        [TestMethod]
        public void Equals_TwoDifferentConverters_False()
        {
            Assert.IsFalse(new BooleanToStringConverter().Equals(new Int32ToStringConverter()));
        }

        [TestMethod]
        public void Equals_TwoSameConverters_True()
        {
            Assert.IsTrue(new BooleanToStringConverter().Equals(new BooleanToStringConverter()));
        }

        [TestMethod]
        public void GetHashCode_TwoDifferentConverters_DifferentHashCodes()
        {
            Assert.IsFalse(new BooleanToStringConverter().GetHashCode() == new Int32ToStringConverter().GetHashCode());
        }

        [TestMethod]
        public void GetHashCode_TwoSameConverters_SameHashCodes()
        {
            Assert.IsTrue(new BooleanToStringConverter().GetHashCode() == new BooleanToStringConverter().GetHashCode());
        }

        [TestMethod]
        public void Equals_ConverterAndNull_False()
        {
            Assert.IsFalse(new BooleanToStringConverter().Equals(null));
        }

        [TestMethod]
        public void GetHashCode_NullConverter_Zero()
        {
            //Assert.AreEqual(0, default(BooleanToStringConverter).GetHashCode());
        }

        [TestMethod]
        public void GetHashCode_SomeConverter_NonZero()
        {
            Assert.IsTrue(new BooleanToStringConverter().GetHashCode() > 0);
        }

        [TestMethod]
        public void Convert_String_String()
        {
            var converter = TypeConverter.Empty.Add<StringToBooleanConverter>();
            var result = converter.Convert("foo", typeof(string));
            result.Verify().IsNotNull().IsTrue(x => (x is string y) && y == "foo");
        }
    }
}
