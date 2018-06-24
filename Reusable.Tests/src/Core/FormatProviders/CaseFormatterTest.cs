using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Extensions;
using Reusable.FormatProviders;
using static Reusable.StringHelper;

namespace Reusable.Tests.FormatProviders
{
    [TestClass]
    public class CaseFormatterTest
    {
        private static readonly IFormatProvider FormatProvider = new CompositeFormatProvider
        {
            typeof(CaseFormatProvider)
        };

        [TestMethod]
        public void Format_String_Upper()
        {
            var bar = "bar";
            Assert.AreEqual("foo BAR baz", Format($"foo {bar:toupper} baz", FormatProvider));
        }

        [TestMethod]
        public void Format_String_Lower()
        {
            var bar = "BAR";
            Assert.AreEqual("foo bar baz", Format($"foo {bar:tolower} baz", FormatProvider));
        }
    }
}
