using JetBrains.Annotations;

namespace Reusable.MarkupBuilder.Html
{
    [PublicAPI]
    public class CssRuleSet
    {
        [NotNull]
        public SoftString Selector { get; set; }

        [NotNull]
        public string Declarations { get; set; }
    }
}