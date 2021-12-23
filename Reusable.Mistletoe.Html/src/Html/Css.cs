using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.Htmlize.Html
{
    [PublicAPI]
    public class Css : List<CssRuleSet>
    {
        public Css()
        {
        }

        public Css(IEnumerable<CssRuleSet> cssRuleSets) : base(cssRuleSets)
        {
        }
    }
}