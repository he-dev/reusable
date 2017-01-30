using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.Converters.Tests
{
    [TestClass]
    public class CompositeConverterTest
    {
        [TestMethod]
        public void ctor_EmptyConverters_CreatesEmptyConverter()
        {
            var converter = new CompositeConverter();
            converter.Count().Verify().IsEqual(0);
        }

        [TestMethod]
        public void ctor_PassTwoDifferentConverters_CreatesWithTwoDifferentConverters()
        {
            var converter = new CompositeConverter(new BooleanToStringConverter(), new Int32ToStringConverter());
            converter.Count().Verify().IsEqual(2);
        }

        [TestMethod]
        public void ctor_PassTwoSameConverters_CreatesWithOneConverter()
        {
            var converter = new CompositeConverter(new BooleanToStringConverter(), new BooleanToStringConverter());
            converter.Count().Verify().IsEqual(1);
        }
    }
}
