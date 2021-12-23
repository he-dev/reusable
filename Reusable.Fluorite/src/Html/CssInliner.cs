using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.Essentials;

namespace Reusable.Fluorite.Html;

public interface ICssInliner
{
    T Inline<T>(ICollection<CssRuleSet> cssRules, T element) where T : IHtmlElement;
}

public class CssInliner : ICssInliner
{
    public T Inline<T>(ICollection<CssRuleSet> cssRules, T element) where T : IHtmlElement
    {
        if (element == null) throw new ArgumentNullException(nameof(element));

        foreach (var child in Element(cssRules, element))
        {
            switch (child)
            {
                case IHtmlElement e:
                    Inline(cssRules, e);
                    break;
            }
        }

        return element;
    }

    private static T Element<T>(IEnumerable<CssRuleSet> cssRules, T element) where T : IHtmlElement
    {
        var selectors = CreateSelectors(element).Distinct();
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
    private static IEnumerable<SoftString> CreateSelectors<T>(T element) where T : IHtmlElement
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

    private static IEnumerable<CssRuleSet> FindCssRules(IEnumerable<CssRuleSet> cssRules, IEnumerable<SoftString> selectors)
    {
        return
            from cssRule in cssRules
            where selectors.Contains(cssRule.Selector)
            select cssRule;
    }

    private static string ConcatenateCssRules(IEnumerable<CssRuleSet> cssRules)
    {
        return string.Join(" ", cssRules.Select(cssRule => $"{cssRule.Declarations.Trim().TrimEnd(';')};"));
    }
}