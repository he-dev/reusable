using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.StringFormatting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using Reusable.StringFormatting.Formatters;

namespace Reusable.Tests
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

            var formattedText = "foo \"bar\" [baz] QUX";

            text.Verify().IsEqual(formattedText);
        }
    }
}
