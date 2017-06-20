using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Markup.Html
{
    public interface ICssInliner
    {
        [NotNull]
        IMarkupElement Inline([NotNull] IEnumerable<CssRule> cssRules, [NotNull] IMarkupElement element);
    }

    public class CssInliner 
    {
        public IMarkupElement Inline(IEnumerable<CssRule> cssRules, IMarkupElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));

            foreach (var child in Element(cssRules, element))
            {
                switch (child)
                {
                    case IMarkupElement e:
                        Inline(cssRules, e);
                        break;
                }
            }

            return element;
        }

        private IMarkupElement Element(IEnumerable<CssRule> cssRules, IMarkupElement element)
        {
            var selectors = CreateSelectors(element).Distinct(StringComparer.OrdinalIgnoreCase);
            var currentCssRules = FindCssRules(cssRules, selectors);
            var style = ConcatenateCssRules(currentCssRules);

            element.Attributes.Remove("style");

            if (!string.IsNullOrEmpty(style))
            {
                element.Attributes.Add("style", style);
            }

            return element;
        }

        // Gets element, #id and .class selectors.
        private static IEnumerable<string> CreateSelectors(IMarkupElement element)
        {
            yield return element.Name;

            if (element.Attributes.TryGetValue("id", out var id))
            {
                yield return $"#{id}";
            }

            if (element.Attributes.TryGetValue("class", out var classes))
            {
                foreach (var className in Regex.Split(classes, @"\s+").Select(className => className.Trim()).Where(Conditional.IsNotNullOrEmpty))
                {
                    yield return $".{className}";
                }
            }
        }

        private IEnumerable<CssRule> FindCssRules(IEnumerable<CssRule> cssRules, IEnumerable<string> selectors)
        {
            return
                from cssRule in cssRules
                where selectors.Contains(cssRule.Selector, StringComparer.OrdinalIgnoreCase)
                select cssRule;
        }

        private static string ConcatenateCssRules(IEnumerable<CssRule> cssRules)
        {
            return string.Join(" ", cssRules.Select(cssRule => $"{cssRule.Declarations.Trim().TrimEnd(';')};"));
        }
    }    
}