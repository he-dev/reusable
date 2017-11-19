using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Formatters;

namespace Reusable.Tests.Formatters
{
    [TestClass]
    public class BracketFormatterTest
    {
        [TestMethod]
        public void Format_SquareBrackets_SquareBrackets()
        {
            var formatter = CustomFormatter.Default().Add<BracketFormatter>();

            var text = string.Format(formatter, "foo {0:square} bar", 20);

            var formattedText = "foo [20] bar";

            Assert.AreEqual(formattedText, text);
        }

        [TestMethod]
        public void Format_CurlyBraces_CurlyBraces()
        {
            var formatter = CustomFormatter.Default().Add<BracketFormatter>();

            var text = string.Format(formatter, "foo {0:curly} bar", 20);

            Assert.AreEqual("foo {20} bar", text);
        }
    }
}
