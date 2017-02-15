using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using Reusable.StringFormatting;
using Reusable.StringFormatting.Formatters;

namespace Reusable.Tests.Formatters
{
    [TestClass]
    public class BracketFormatterTest
    {
        [TestMethod]
        public void Format_SquareBrackets_SquareBrackets()
        {
            var formatter = CustomFormatter.Default().Add<BracketFormatter>();

            var text = string.Format(formatter, "foo {0:[]} bar", 20);

            var formattedText = "foo [20] bar";

            Assert.AreEqual(formattedText, text);
        }

        [TestMethod]
        public void Format_CurlyBraces_CurlyBraces()
        {
            var formatter = CustomFormatter.Default().Add<BracketFormatter>();

            var text = string.Format(formatter, "foo {0:{{}}} bar", 20);

            var formattedText = "foo {20} bar";

            text.Verify().IsEqual(formattedText);
        }
    }
}
