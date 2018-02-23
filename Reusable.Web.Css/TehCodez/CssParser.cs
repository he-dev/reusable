using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Web
{
    public interface ICssParser
    {
        [NotNull]
        Css Parse([NotNull] string css);
    }

    public class CssParser : ICssParser
    {
        public static readonly ICssParser Default = new CssParser();
        
        public Css Parse(string css)
        {
            if (css == null) throw new ArgumentNullException(nameof(css));

            var cssRulesets = ParseRulesets(css);
            return new Css(cssRulesets);
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<CssRuleset> ParseRulesets(string css)
        {
            css = RemoveLineBreaks(css);

            // https://regex101.com/r/iJ8MZX/3

            var rulesetMatches =
                Regex
                    .Matches(css, @"(?<selectors>[a-z0-9_\-\.,\s#]+)\s*{(?<declarations>.+?)}", RegexOptions.IgnoreCase)
                    .Cast<Match>();

            return
                from rulesetMatch in rulesetMatches
                select new CssRuleset
                {
                    Selectors = SplitSelectors(rulesetMatch.Groups["selectors"].Value).ToList(),
                    Declarations = rulesetMatch.Groups["declarations"].Value.Trim()
                };
        
            string RemoveLineBreaks(string value) => Regex.Replace(value, @"(\r\n|\r|\n)", string.Empty);
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<string> SplitSelectors(string selectors)
        {
            return
                Regex
                    .Split(selectors, ",")
                    .Where(Conditional.IsNotNullOrEmpty)
                    .Select(selector => selector.Trim().ToLowerInvariant());
        }
    }
}