using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Formatters;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

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

            text.Verify().IsEqual(formattedText);
        }

        [TestMethod]
        public void Format_String_Lower()
        {
            var formatter = CustomFormatter.Default().Add<CaseFormatter>();

            var text = string.Format(formatter, "foo {0:tolower} baz", "BAR");

            var formattedText = "foo bar baz";

            text.Verify().IsEqual(formattedText);
        }
    }
}
