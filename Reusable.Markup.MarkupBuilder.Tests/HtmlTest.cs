using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Data;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using Reusable.Markup.Html;

namespace Reusable.Markup.Tests
{
    [TestClass]
    public class HtmlTest
    {
        private static readonly IMarkupElement HtmlBuilder = MarkupElement.Builder;

        private static readonly IMarkupFormatting Formatting = new MarkupFormatting(
            MarkupFormattingTemplate.Parse(ResourceReader.ReadEmbeddedResource("Reusable.Markup.Tests.Resources.FormattingTemplate.html")));

        [TestMethod]
        public void ToString_001()
        {
            var html = HtmlBuilder.Element("h1").ToHtml(Formatting);
            Assert.AreEqual(Expected(), html);
        }

        [TestMethod]
        public void ToString_002()
        {
            var html = HtmlBuilder.Element("h1", h1 => h1.Element("span")).ToHtml(Formatting);
            Assert.AreEqual(Expected(), html);
        }

        [TestMethod]
        public void ToString_003()
        {
            var html = HtmlBuilder
                .Element("body", body => body
                    .Element("p", p => p
                        .Append("foo ")
                        .Element("span", span => span
                            .Attribute("class", "quux")
                            .Append("bar"))
                        .Append(" baz"))
                    .Element("p", p => p
                        .Append("foo ")
                        .Element("span", "qux")
                        .Append(" baz")))
                .ToHtml(Formatting);
            Assert.AreEqual(Expected(), html);
        }

        [TestMethod]
        public void ToString_004()
        {
            var html = HtmlBuilder
                .Element("table", table => table
                    .Element("thead", thead => thead
                        .Element("tr", tr => tr
                            .Elements("th", new[] { "foo", "bar", "baz" }, (th, x) => th.Append(x))))
                    .Element("tbody", tbody => tbody
                        .Element("tr", tr => tr
                            .Element("td", "foo1")
                            .Element("td", "bar1")
                            .Element("td", "baz1"))
                        .Element("tr", tr => tr
                            .Element("td", string.Empty)
                            .Element("td", default(string))
                            .Element("td", string.Empty)))
                    .Element("tfoot", tfoot => tfoot
                        .Element("tr", tr => tr
                            .Elements("td", new[] { "foo", "bar", "baz" }, (td, x) => td.Append(x)))))
                .ToHtml(Formatting);
            Assert.AreEqual(Expected().Trim(), html.Trim());
        }

        [TestMethod]
        public void ToString_005()
        {
            var html = HtmlBuilder
                .Element("ul", ul => ul.Elements("li", new object[] { "foo", "bar", "baz" }, (li, x) => li.Append(x))
            ).ToHtml(Formatting);
            Assert.AreEqual(Expected().Trim(), html.Trim());
        }

        [TestMethod]
        public void ToString_006()
        {
            var dataTable = new DataTable().AddColumn("value").AddRow("foo").AddRow("bar").AddRow("baz").AddRow("qux");

            var html =
                HtmlBuilder
                    .Element("ul", ul => ul
                        .Elements("li", dataTable.AsEnumerable().Take(3).Select(x => x.Field<string>("value")), (li, x) => li.Append(x)))
                .ToHtml(Formatting);
            Assert.AreEqual(Expected().Trim(), html.Trim());
        }

        [TestMethod]
        public void ToString_007()
        {
            var data = new[]
            {
                new[] {1, 2, 3},
                new[] {4, 5, 6},
            };

            var html = HtmlBuilder
                .Element("table", table => table
                    .Element("tbody", tbody => tbody
                        .Elements("tr", data, (tr, row) => tr
                            .Elements("td", row, (td, x) => td.Append(x)))));
            //.ToHtml();
            Assert.AreEqual(Expected().Trim(), html.ToHtml(Formatting).Trim());
        }

        private static string Expected([CallerMemberName] string memberName = null)
        {
            return ResourceReader.ReadEmbeddedResource<HtmlTest>(memberName);
        }
    }
}
