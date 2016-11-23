using System;
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
            Html = new MarkupBuilder(new HtmlRenderer());
        }

        [TestMethod]
        public void EmptyElement()
        {
            ((string) Html.h1()).Verify().IsEqual("<h1></h1>");
        }

        [TestMethod]
        public void NestedElements()
        {
            ((string)Html.h1(Html.span())).Verify().IsEqual("<h1><span></span></h1>");
        }

        [TestMethod]
        public void createElement()
        {
            dynamic html = new MarkupBuilder(new HtmlRenderer())
                .Extensions.Add<createElementExtension>();

            var h1 = (string)html.createElement("h1", "foo").ToString();
            h1.Verify(nameof(h1)).IsEqual("<h1>foo</h1>");
        }
    }
}
