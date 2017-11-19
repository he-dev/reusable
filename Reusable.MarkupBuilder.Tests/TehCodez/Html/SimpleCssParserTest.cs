using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Data.Repositories;
using Reusable.MarkupBuilder.Html;
using Reusable.Reflection;

namespace Reusable.MarkupBuilder.Tests.Html
{
    [TestClass]
    public class SimpleCssParserTest
    {
        [TestMethod]
        public void Parse_Styles_Styles()
        {
            var css = new CssParser().Parse(ResourceReader<SimpleCssParserTest>.FindString(name => name.EndsWith("styles.css", StringComparison.OrdinalIgnoreCase)));
            Assert.AreEqual(4, css.Count());            
        }
    }
}
