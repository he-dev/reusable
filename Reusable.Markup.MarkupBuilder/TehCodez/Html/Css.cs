using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Markup.Html
{
    public interface ICssParser
    {
        [NotNull]
        Css Parse([NotNull] string css);
    }

    public class SimpleCssParser : ICssParser
    {
        public Css Parse(string css)
        {
            if (css == null) throw new ArgumentNullException(nameof(css));

            var cssRules = MatchRules(css);
            return new Css(cssRules);
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<CssRule> MatchRules(string css)
        {
            // https://regex101.com/r/iJ8MZX/3
            return
                from m in Regex
                    .Matches(css.Minify(), @"(?<selectors>[a-z0-9_\-\.,\s#]+)\s*{(?<declarations>.+?)}", RegexOptions.IgnoreCase)
                    .Cast<Match>()
                from selector in SplitSelectors(m.Groups["selectors"].Value)
                select new CssRule
                {
                    Selector = selector,
                    Declarations = m.Groups["declarations"].Value.Trim()
                };
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<string> SplitSelectors(string selectors)
        {
            return
                Regex
                    .Split(selectors, @",")
                    .Where(Conditional.IsNotNullOrEmpty)
                    .Select(selector => selector.Trim().ToLowerInvariant());
        }
    }

    [PublicAPI]
    public class CssRule
    {
        [NotNull]
        public string Selector { get; set; }

        [NotNull]
        public string Declarations { get; set; }
    }

    [PublicAPI]
    public class Css : IEnumerable<CssRule>
    {
        private readonly List<CssRule> _cssRules = new List<CssRule>();

        public Css() { }

        public Css([NotNull] IEnumerable<CssRule> cssRules)
        {
            if (cssRules == null) throw new ArgumentNullException(nameof(cssRules));
            _cssRules.AddRange(cssRules);
        }

        public void Add([NotNull] CssRule cssRule)
        {
            if (cssRule == null) throw new ArgumentNullException(nameof(cssRule));
            _cssRules.Add(cssRule);
        }

        public IEnumerator<CssRule> GetEnumerator() => _cssRules.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal static class StringExtensions
    {
        /// <summary>
        /// Removes line breakes form a string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Minify(this string value) => Regex.Replace(value, @"(\r\n|\r|\n)", string.Empty);

        public static IEnumerable<string> ToLines(this string template)
        {
            return
                Regex
                    .Split(template, @"(\r\n|\r|\n)")
                    // Skip empty lines.
                    .Where(line => !string.IsNullOrEmpty(line.Trim()));
        }
    }
}