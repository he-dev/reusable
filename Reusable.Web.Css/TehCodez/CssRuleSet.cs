using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.Web
{
    [PublicAPI]
    public class CssRuleset
    {
        [NotNull]
        public IList<string> Selectors { get; set; } = new List<string>();

        [NotNull]
        public string Declarations { get; set; }
    }
}