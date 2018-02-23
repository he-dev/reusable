using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Reflection;

namespace Reusable.Web.Css.Tests
{
    [TestClass]
    public class CssParserTest
    {
        [TestMethod]
        public void Parse_Styles_Styles()
        {
            var css = new CssParser().Parse(ResourceReader.Default.FindString("styles.css"));
            Assert.AreEqual(4, css.Count());
        }
    }
}
