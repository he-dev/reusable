using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Utilities.MSTest;

namespace Reusable.Converters.Tests
{
    [TestClass]
    public class CompositeConverterTest
    {
        [TestMethod]
        public void ctor_EmptyConverters_CreatesEmptyConverter()
        {
            var converter = new CompositeConverter();
            Assert.That.Collection().IsEmpty(converter);
        }

        [TestMethod]
        public void ctor_PassTwoDifferentConverters_CreatesWithTwoDifferentConverters()
        {
            var converter = new CompositeConverter(new BooleanToStringConverter(), new Int32ToStringConverter());
            Assert.That.Collection().CountEquals(2, converter);
        }

        [TestMethod]
        public void ctor_PassTwoSameConverters_CreatesWithTwoConverters()
        {
            var converter = new CompositeConverter(new BooleanToStringConverter(), new BooleanToStringConverter());
            Assert.That.Collection().CountEquals(2, converter);
        }
    }
}
