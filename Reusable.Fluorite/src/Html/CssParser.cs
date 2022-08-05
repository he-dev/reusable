using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.Marbles;

namespace Reusable.Fluorite.Html;

public interface ICssParser
{
    Css Parse(string css);
}

public class CssParser : ICssParser
{
    public static readonly ICssParser Default = new CssParser();
        
    public Css Parse(string css)
    {
        if (css == null) throw new ArgumentNullException(nameof(css));

        var cssRules = MatchRules(css);
        return new Css(cssRules);
    }

    private static IEnumerable<CssRuleSet> MatchRules(string css)
    {
        css = RemoveLineBreaks(css);
            
        // https://regex101.com/r/iJ8MZX/3
        return
            from selectorMatches in Regex
                .Matches(css, @"(?<selectors>[a-z0-9_\-\.,\s#]+)\s*{(?<declarations>.+?)}", RegexOptions.IgnoreCase)
                .Cast<Match>()
            from selector in SplitSelectors(selectorMatches.Groups["selectors"].Value)
            select new CssRuleSet
            {
                Selector = selector,
                Declarations = selectorMatches.Groups["declarations"].Value.Trim()
            };
        
        string RemoveLineBreaks(string value) => Regex.Replace(value, @"(\r\n|\r|\n)", string.Empty);
    }

    private static IEnumerable<string> SplitSelectors(string selectors)
    {
        return
            Regex
                .Split(selectors, @",")
                .Where(Conditional.IsNotNullOrEmpty)
                .Select(selector => selector.Trim().ToLowerInvariant());
    }
}