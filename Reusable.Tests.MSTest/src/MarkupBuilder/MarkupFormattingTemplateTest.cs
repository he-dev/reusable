using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.IO.Extensions;
using Reusable.MarkupBuilder;
using Reusable.Reflection;

namespace Reusable.Tests.MarkupBuilder
{
    using static Helper;

    [TestClass]
    public class MarkupFormattingTemplateTest
    {
        [TestMethod]
        public void Parse_FormattingTemplate_Options()
        {
            var template = ResourceProvider.GetFileInfoAsync("FormattingTemplate.html").Result.ReadAllText();

            Assert.IsNotNull(template);

            var options = MarkupFormattingTemplate.Parse(template);

            Assert.IsTrue(options.TryGetValue("table", out var o) && o == MarkupFormattingOptions.PlaceBothTagsOnNewLine);
        }
    }
}
