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

            Assert.AreEqual(@"<p>foo <span class=""qux"">bar</span> baz</p>", html.ToHtml(MarkupFormatting.Empty));            

            var cssRules = new[] {new CssRule {Selector = ".qux", Declarations = "font-family: sans-serif;"}};

            html = new CssInliner().Inline(cssRules, html);

            Assert.AreEqual(@"<p>foo <span class=""qux"" style=""font-family: sans-serif;"">bar</span> baz</p>", html.ToHtml(MarkupFormatting.Empty));
        }

        [TestMethod]
        public void Visit_Table_Applied()
        {
            var table = Html.Element("table").Class("foo");

            var tbody = Html.Element("tbody");
            var tr = Html.Element("tr");

            tr.Element("td", td => td.Class("bar foo").Append("baz1"));
            tr.Element("td", td => td.Class("bar").Append("baz2"));
            
            table.Add(tbody);
            tbody.Add(tr);            

            var cssRules = new[]
            {
                new CssRule { Selector = ".foo", Declarations = "font-family: sans-serif;" },
                new CssRule { Selector = ".bar", Declarations = "font-family: consolas;" }
            };

            table = new CssInliner().Inline(cssRules, table);

            var result = table.ToHtml(MarkupFormatting.Empty);

            Assert.AreEqual(@"<p>foo <span class=""qux"" style=""font-family: sans-serif;"">bar</span> baz</p>", result);
            /*
             <table class="foo" style="font-family: sans-serif;"><tbody><tr><td class="bar foo" style="font-family: sans-serif;font-family: consolas;">baz1</td><td class="bar" style="font-family: consolas;">baz2</td></tr></tbody></table>
             
             */
        }
    }
}
