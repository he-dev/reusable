using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Data.Repositories;
using Reusable.MarkupBuilder;
using Reusable.MarkupBuilder.Html;
using Reusable.Reflection;

namespace Reusable.MarkupBuilder.Tests.Html
{
    [TestClass]
    public class CssInlinerTest
    {
        private static readonly HtmlElement Html = HtmlElement.Builder;

        private static readonly HtmlFormatting Formatting = HtmlFormatting.Parse(ResourceReader<MarkupFormattingTemplateTest>.FindString(name => name.Contains("FormattingTemplate")));

        [TestMethod]
        public void Inline_SpanStyles_Inlined()
        {
            var html = Html
                .Element("p", p => p
                    .Append("foo ")
                    .Element("span", span => span
                        .Attribute("class", "qux")
                        .Append("bar"))
                    .Append(" baz"));

            Assert.AreEqual(@"<p>foo <span class=""qux"">bar</span> baz</p>", html.ToHtml(HtmlFormatting.Empty));

            var cssRules = new[] { new CssRuleSet { Selector = ".qux", Declarations = "font-family: sans-serif;" } };

            html = new CssInliner().Inline(cssRules, html);

            Assert.AreEqual(@"<p>foo <span class=""qux"" style=""font-family: sans-serif;"">bar</span> baz</p>", html.ToHtml(HtmlFormatting.Empty));
        }

        [TestMethod]
        public void Inline_TableStyles_Inlined()
        {
            var table = Html.Element("table").@class("foo");

            var tbody = Html.Element("tbody");
            var tr = Html.Element("tr");

            MarkupElementExtensions.Element(tr, "td", td => td.@class("bar foo").Append("baz1"));
            MarkupElementExtensions.Element(tr, "td", td => td.@class("bar").Append("baz2"));

            table.Add(tbody);
            tbody.Add(tr);

            var cssRules = new[]
            {
                new CssRuleSet { Selector = ".foo", Declarations = "font-family: sans-serif;" },
                new CssRuleSet { Selector = ".bar", Declarations = "font-family: consolas;" }
            };

            table = new CssInliner().Inline(cssRules, table);

            var result = table.ToHtml(Formatting);

            Assert.AreEqual(ResourceReader<CssInlinerTest>.FindString(name => name.Contains("CssInliner_Inline_TableStyles")).Trim(), result.Trim());
        }
    }
}
