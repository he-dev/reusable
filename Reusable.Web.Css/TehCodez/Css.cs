using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.Web
{
    [PublicAPI]
    public class Css : List<CssRuleset>
    {
        public Css()
        {
        }

        public Css(IEnumerable<CssRuleset> cssRuleSets) : base(cssRuleSets)
        {
        }
    }
}