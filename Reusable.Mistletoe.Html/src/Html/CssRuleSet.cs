using JetBrains.Annotations;

namespace Reusable.Htmlize.Html
{
    [PublicAPI]
    public class CssRuleSet
    {
        public SoftString Selector { get; set; }

        public string Declarations { get; set; }
    }
}