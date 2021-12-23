using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.Fluorite.Html;

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