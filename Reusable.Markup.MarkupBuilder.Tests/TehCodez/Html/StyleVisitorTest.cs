using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Markup.Html;

namespace Reusable.Markup.Tests.Html
{
    [TestClass]
    public class StyleVisitorTest
    {
        private static readonly IMarkupElement Html = MarkupElement.Builder;

        private static readonly IMarkupFormatting Formatting = new MarkupFormatting(
            MarkupFormattingTemplate.Parse(ResourceReader.ReadEmbeddedResource<MarkupFormattingTemplateTest>("Resources.FormattingTemplate.html")));

        [TestMethod]
        public void Visit_WithStyles_Applied()
        {
            var html = Html
                .Element("p", p => p
                    .Append("foo ")
                    .Element("span", span => span
                        .Attribute("class", "qux")
                        .Append("bar"))
                    .Append(" baz"));

            Assert.AreEqual(@"<p>foo <span class=""qux"">bar</span> baz</p>", html.ToHtml(Formatting));

            var styleVisitor = new StyleVisitor(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [".qux"] = "font-family: sans-serif;"
            });

            html = styleVisitor.Visit(html);

            Assert.AreEqual(@"<p>foo <span class=""qux"" style=""font-family: sans-serif;"">bar</span> baz</p>", html.ToHtml(Formatting));
        }

        [TestMethod]
        public void Visit_Table_Applied()
        {
            var table = Html.Element("table").Class("foo");

            var tbody = Html.Element("tbody");
            var tr = Html.Element("tr");

            tr.Element("td", td => td.Class("bar").Append("baz1"));
            tr.Element("td", td => td.Class("bar").Append("baz2"));
            
            table.Add(tbody);
            tbody.Add(tr);            

            var styleVisitor = new StyleVisitor(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [".foo"] = "font-family: sans-serif;",
                [".bar"] = "font-family: consolas;"
            });

            table = styleVisitor.Visit(table);
            var result = table.ToHtml(Formatting);

            Assert.AreEqual(@"<p>foo <span class=""qux"" style=""font-family: sans-serif;"">bar</span> baz</p>", result);
        }
    }
}
