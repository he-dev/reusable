using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Markup.Extensions;
using Reusable.Markup.Renderers;
using Reusable.Testing;
using Reusable.Validations;

namespace Reusable.Markup.Tests
{
    [TestClass]
    public class ToString
    {
        [TestMethod]
        public void createElement()
        {
            dynamic html = new MarkupBuilder(new HtmlRenderer())
                .Add<createElementExtension>();

            var h1 = (string)html.createElement("h1", "foo").ToString();
            h1.Verify(nameof(h1)).IsEqual("<h1>foo</h1>");
        }
    }
}
