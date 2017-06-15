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
            return new Css(MatchRules(css));
        }

        [NotNull]
        [ItemNotNull]
        private static IEnumerable<CssRule> MatchRules(string css)
        {
            return
                Regex
                    .Matches(css.Minify(), @"(?<selectors>[a-z0-9_\-\.,\s#]+)\s*{(?<declarations>.+?)}", RegexOptions.IgnoreCase)
                    .Cast<Match>()
                    .Select(m => new CssRule(
                            selectors: SplitSelectors(m.Groups["selectors"].Value),
                            declarations: m.Groups["declarations"].Value.Trim()));
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
        private ISet<string> _selectors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public CssRule() { }

        public CssRule([NotNull] IEnumerable<string> selectors, [NotNull] string declarations)
        {
            if (selectors == null) throw new ArgumentNullException(nameof(selectors));

            Selectors = new HashSet<string>(selectors, StringComparer.OrdinalIgnoreCase);
            Declarations = declarations ?? throw new ArgumentNullException(nameof(declarations));
        }

        [NotNull]
        [ItemNotNull]
        public ISet<string> Selectors
        {
            get => _selectors;
            set => _selectors = value ?? throw new ArgumentNullException(nameof(Selectors));
        }

        [CanBeNull]
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

        public static implicit operator Dictionary<string, string>(Css css)
        {
            return
                css
                    .SelectMany(rule => rule
                        .Selectors
                        .Select(selector => new KeyValuePair<string, string>(selector, rule.Declarations)))
                    .ToDictionary(x => x.Key, x => x.Value);
        }
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