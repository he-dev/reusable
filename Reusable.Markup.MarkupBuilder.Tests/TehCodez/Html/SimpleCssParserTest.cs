using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Markup.Html;

namespace Reusable.Markup.Tests.Html
{
    [TestClass]
    public class SimpleCssParserTest
    {
        [TestMethod]
        public void Parse_Styles_Styles()
        {
            var css = new SimpleCssParser().Parse(ResourceReader.ReadEmbeddedResource("Reusable.Markup.Tests.Resources.styles.css"));
            Assert.AreEqual(4, css.Count());            
        }
    }
}
