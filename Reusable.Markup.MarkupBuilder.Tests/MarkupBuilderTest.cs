using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using Reusable.Markup.Extensions;
using Reusable.Markup.Renderers;

namespace Reusable.Markup.Tests
{
    [TestClass]
    public class MarkupBuilderTest
    {
        private dynamic Html { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            var builder = new MarkupBuilder(new HtmlRenderer());
            builder.Extensions.Add<attrExtension>();
            builder.Extensions.Add<cssExtension>();
            Html = builder;
        }

        [TestMethod]
        public void ToString_001()
        {
            var html = Html.h1();
            Assert.AreEqual(html.ToString(), Expected());
        }

        [TestMethod]
        public void ToString_002()
        {
            var html = Html.h1(Html.span());
            Assert.AreEqual(html.ToString(), Expected());
        }

        [TestMethod]
        public void ToString_003()
        {
            var html = Html.body(
                Html.p("foo ", Html.span("bar"), " baz"),
                Html.p("foo ", Html.span("qux"), " baz")
            );
            Assert.AreEqual((string)html.ToString(), Expected());
        }

        private static string Expected([CallerMemberName] string memberName = null)
        {
            return File.ReadAllText($@"HtmlFiles\{memberName}.html");
        }
    }
}
