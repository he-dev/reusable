using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Formatters;

namespace Reusable.Tests.Formatters
{
    [TestClass]
    public class CaseFormatterTest
    {
        [TestMethod]
        public void Format_String_Upper()
        {
            var formatter = CustomFormatter.Default().Add<CaseFormatter>();

            var text = string.Format(formatter, "foo {0:upper} baz", "bar");

            var formattedText = "foo BAR baz";

            Assert.AreEqual(formattedText, text);
        }

        [TestMethod]
        public void Format_String_Lower()
        {
            var formatter = CustomFormatter.Default().Add<CaseFormatter>();

            var text = string.Format(formatter, "foo {0:tolower} baz", "BAR");

            Assert.AreEqual("foo bar baz", text);
        }
    }
}
