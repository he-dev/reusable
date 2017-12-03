using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Extensions;
using System.Linq.Custom;

namespace Reusable.MarkupBuilder.Html
{
    public static class CssExtensions
    {
        private const string DeclarationPattern = @"(?<property>[a-z-]+):\s*(?<value>.+?)(;|$)";

        private static readonly Regex DeclarationRegex = new Regex(DeclarationPattern, RegexOptions.IgnoreCase);

        public static IEnumerable<(string property, string value)> ToDeclarations([NotNull] this string style)
        {
            if (style == null) throw new ArgumentNullException(nameof(style));
            
            return
                DeclarationRegex
                    .Matches(style)
                    .Cast<Match>()
                    .Select(m => (
                        property: m.Value("property").Trim(),
                        value: m.Value("value").Trim())
                    );
        }

        public static IEnumerable<(string property, string value)> AddOrUpdate([NotNull] this IEnumerable<(string property, string value)> declarations, params (string property, string value)[] updates)
        {
            if (declarations == null) throw new ArgumentNullException(nameof(declarations));
            
            return
                declarations
                    .Concat(updates)
                    .GroupBy(t => t.property)
                    .Select(t => t.Last())
                    .Skip(t => string.IsNullOrEmpty(t.value));
        }

        public static string ToStyle([NotNull] this IEnumerable<(string property, string value)> declarations)
        {
            if (declarations == null) throw new ArgumentNullException(nameof(declarations));
            
            return
                declarations
                    .Select(t => $"{t.property}: {t.value};")
                    .Join(" ");
        }
    }
}