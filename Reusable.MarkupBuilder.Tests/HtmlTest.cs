using System;
using System.Data;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Data;
using Reusable.Data.Repositories;
using Reusable.MarkupBuilder;
using Reusable.MarkupBuilder.Html;
using Reusable.Reflection;

namespace Reusable.MarkupBuilder.Tests
{
    [TestClass]
    public class HtmlTest
    {
        private static readonly HtmlElement HtmlBuilder = HtmlElement.Builder;

        private static readonly HtmlFormatting Formatting = HtmlFormatting.Parse(ResourceReader.Default.FindString(name => name.Contains("FormattingTemplate")));

        [TestMethod]
        public void ToString_001()
        {
            var html = HtmlBuilder.Element("h1").ToHtml(Formatting);
            Assert.AreEqual(ResourceReader.Default.FindString(name => name.Contains(nameof(ToString_001))), html);
        }

        [TestMethod]
        public void ToString_002()
        {
            var html = HtmlBuilder.Element("h1", h1 => h1.Element("span")).ToHtml(Formatting);
            Assert.AreEqual(ResourceReader.Default.FindString(name => name.Contains(nameof(ToString_002))), html);
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
            Assert.AreEqual(ResourceReader.Default.FindString(name => name.Contains(nameof(ToString_003))), html);
        }

        [TestMethod]
        public void ToString_004()
        {
            var html = HtmlBuilder
                .Element("table", table => table
                    .Element("thead", thead => thead
                        .Element("tr", tr => tr
                            .Elements("th", new[] { "foo", "bar", "baz" }, (th, x) => th.Add(x))))
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
            Assert.AreEqual(
                ResourceReader.Default.FindString(name => name.Contains(nameof(ToString_004))).Trim(), 
                html.Trim());
        }

        [TestMethod]
        public void ToString_005()
        {
            var html = HtmlBuilder
                .Element("ul", ul => ul.Elements("li", new object[] { "foo", "bar", "baz" }, (li, x) => li.Append(x))
            ).ToHtml(Formatting);
            Assert.AreEqual(
                ResourceReader.Default.FindString(name => name.Contains(nameof(ToString_005))).Trim(), 
                html.Trim());
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
            Assert.AreEqual(
                ResourceReader.Default.FindString(name => name.Contains(nameof(ToString_006))).Trim(), 
                html.Trim());
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
            Assert.AreEqual(
                ResourceReader.Default.FindString(name => name.Contains(nameof(ToString_007))).Trim(), 
                html.ToHtml(Formatting).Trim());
        }
    }
}
