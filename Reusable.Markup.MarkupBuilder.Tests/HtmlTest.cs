using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using Reusable.Markup.Html;

namespace Reusable.Markup.Tests
{
    [TestClass]
    public class HtmlTest
    {
        private static readonly IMarkupElement Html = MarkupElement.Builder;

        [TestMethod]
        public void ToString_001()
        {
            var html = Html.h1(string.Empty).ToHtml();
            Assert.AreEqual(Expected(), html);
        }

        [TestMethod]
        public void ToString_002()
        {
            var html = Html.h1(Html.span(string.Empty)).ToHtml();
            Assert.AreEqual(Expected(), html);
        }

        [TestMethod]
        public void ToString_003()
        {
            var html = Html.body(
                Html.p("foo ", Html.span("bar"), " baz"),
                Html.p("foo ", Html.span("qux"), " baz")
            ).ToHtml();
            Assert.AreEqual(Expected(), html);
        }

        [TestMethod]
        public void ToString_004()
        {
            var html = Html.table(
                Html.thead(thead => thead.tr(tr => tr.th("foo"), tr => tr.th("bar"), tr => tr.th("baz"))),
                Html.tbody(
                    Html.tr(Html.td("foo1"), Html.td("bar1"), Html.td("baz1")),
                    Html.tr(Html.td(string.Empty), Html.td(null), Html.td(string.Empty))),
                Html.tfoot(Html.tr(Html.td("foo"), Html.td("bar"), Html.td("baz")))
            ).ToHtml();
            Assert.AreEqual(Expected().Trim(), html.Trim());
        }

        private static string Expected([CallerMemberName] string memberName = null)
        {
            return ResourceReader.ReadEmbeddedResource<HtmlTest>($@"Resources.{memberName}.html");
        }
    }
}
