using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.Markup.Tests
{
    [TestClass]
    public class MarkupFormattingTemplateTest
    {
        [TestMethod]
        public void Parse_FormattingTemplate_Options()
        {
            var template = ResourceReader.ReadEmbeddedResource<MarkupFormattingTemplateTest>("Resources.FormattingTemplate.html");

            Assert.IsNotNull(template);

            var options = MarkupFormattingTemplate.Parse(template);

            Assert.IsTrue(options.TryGetValue("table", out var o) && o == MarkupFormattingOptions.PlaceBothTagsOnNewLine);
        }
    }
}
