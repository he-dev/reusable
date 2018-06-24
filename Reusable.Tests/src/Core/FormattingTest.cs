using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Extensions;
using Reusable.FormatProviders;
using static Reusable.StringHelper;

namespace Reusable.Tests
{
    [TestClass]
    public class FormattingTest
    {
        [TestMethod]
        public void Format_Composite_Formatted()
        {
            Assert.AreEqual("foo 'string' bar [1.50]", Format($"foo {typeof(string):lower|single} bar {1.5.ToString("F2", CultureInfo.InvariantCulture):square}", new CompositeFormatProvider
            {
                typeof(PunctuationFormatProvider),
                typeof(TypeFormatProvider),
                typeof(CaseFormatProvider),
            }));

            Assert.AreEqual("foo 'STRING' bar", Format($"foo {typeof(string):upper|single} bar", new CompositeFormatProvider
            {
                typeof(PunctuationFormatProvider),
                typeof(CaseFormatProvider),
                typeof(TypeFormatProvider),
                CultureInfo.InvariantCulture,
            }));
        }

        [TestMethod]
        public void Format_AllFormats_Formatted()
        {
            var formatProvider = new CompositeFormatProvider
            {
                typeof(HexColorFormatProvider)
            };

            //var formatter = CustomFormatter.Default()
            //    .Add<CaseFormatter>()
            //    .Add<BracketFormatter>()
            //    .Add<QuoteFormatter>()
            //    .Add<DecimalColorFormatter>()
            //    .Add<HexColorFormatProvider>();

            var text = "foo {a:double} {b:square} {c:u}".Format(new
            {
                a = "bar",
                b = "baz",
                c = "qux"
            }, formatProvider);

            Assert.AreEqual("foo \"bar\" [baz] QUX", text);
        }
    }
}
