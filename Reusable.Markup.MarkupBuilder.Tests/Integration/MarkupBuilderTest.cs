using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using Reusable.Markup.Extensions;
using Reusable.Markup.Renderers;

namespace Reusable.Markup.Tests.Integration
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
            //builder.Extensions.Add<styleExtension>()
            builder.Extensions.Add<cssExtension>();
            builder.Extensions.Add<createElementExtension>();
            Html = builder;
        }

        [TestMethod]
        public void ToString_EmptyElement()
        {
            ((string)Html.h1()).Verify().IsEqual("<h1></h1>");
        }

        [TestMethod]
        public void ToString_NestedElements()
        {
            ((string)Html.h1(Html.span())).Verify().IsEqual("<h1><span></span></h1>");
        }

        // --- Extensions

        [TestMethod]
        public void createElement()
        {
           ((string)Html.createElement("h1", "foo")).Verify().IsEqual("<h1>foo</h1>");
        }
    }
}
