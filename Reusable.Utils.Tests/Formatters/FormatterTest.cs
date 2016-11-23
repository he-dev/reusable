using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Formatters;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.Tests
{
    [TestClass]
    public class FormatterTest
    {
        [TestMethod]
        public void AllFormats()
        {
            var formatter = Formatter.Default()
                .Add<CaseFormatter>()
                .Add<BracketFormatter>()
                .Add<QuoteFormatter>()
                .Add<DecimalColorFormatter>()
                .Add<HexadecimalColorFormatter>();

            var text = "foo {a:dq} {b:sb} {c:u}".Format(new
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
