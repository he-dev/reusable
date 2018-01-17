using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Data.Repositories;
using Reusable.Reflection;

namespace Reusable.MarkupBuilder.Tests
{
    [TestClass]
    public class MarkupFormattingTemplateTest
    {
        [TestMethod]
        public void Parse_FormattingTemplate_Options()
        {
            var template = ResourceReader.Default.FindString(name => name.Contains("FormattingTemplate"));

            Assert.IsNotNull(template);

            var options = MarkupFormattingTemplate.Parse(template);

            Assert.IsTrue(options.TryGetValue("table", out var o) && o == MarkupFormattingOptions.PlaceBothTagsOnNewLine);
        }
    }
}
