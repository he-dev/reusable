using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.IOnymous;
using Reusable.MarkupBuilder.Html;

namespace Reusable.Tests.MarkupBuilder.Html
{
    using static Helper;

    [TestClass]
    public class SimpleCssParserTest
    {
        [TestMethod]
        public void Parse_Styles_Styles()
        {
            var css = new CssParser().Parse(ResourceProvider.ReadTextFile("styles.css"));
            Assert.AreEqual(6, css.Count());            
        }
    }
}
