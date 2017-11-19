using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Extensions;
using Reusable.Formatters;

namespace Reusable.Tests.Formatters
{
    [TestClass]
    public class FormatterTest
    {
        [TestMethod]
        public void AllFormats()
        {
            var formatter = CustomFormatter.Default()
                .Add<CaseFormatter>()
                .Add<BracketFormatter>()
                .Add<QuoteFormatter>()
                .Add<DecimalColorFormatter>()
                .Add<HexadecimalColorFormatter>();

            var text = "foo {a:double} {b:square} {c:u}".Format(new
            {
                a = "bar",
                b = "baz",
                c = "qux"
            }, formatter);

            Assert.AreEqual("foo \"bar\" [baz] QUX", text);
        }
    }
}
