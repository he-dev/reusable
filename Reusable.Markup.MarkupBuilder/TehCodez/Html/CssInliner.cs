using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Markup.Html
{
    //public interface IMarkupModifier
    //{
    //    [NotNull]
    //    IMarkupElement Apply([NotNull] IMarkupElement element);
    //}

    public interface ICssInliner
    {
        [NotNull]
        IMarkupElement Inline([NotNull] IEnumerable<CssRule> cssRules, [NotNull] IMarkupElement element);
    }

    public class CssInliner // : IMarkupModifier
    {
        //private readonly IDictionary<string, string> _styles;

        //public CssInliner()
        //{
        //    //_styles = styles ?? throw new ArgumentNullException(nameof(styles));
        //}

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
            var styles = new StringBuilder();

            foreach (var cssRule in cssRules)
            {
                // Fix the ";" but trim it first in case there is already one to avoid an "if".
                styles
                    .Append(cssRule.Declarations.Trim().TrimEnd(';'))
                    .Append(";");
            }
            return styles.ToString();
        }
    }

    //public abstract class MarkupVisitor : IMarkupVisitor
    //{
    //    public abstract IMarkupElement Visit(IMarkupElement element);

    //    protected abstract IMarkupElement Element(IMarkupElement element);

    //    protected abstract string Text(string text);
    //}

    //public class MultiVisitor : MarkupVisitor
    //{
    //    private readonly IEnumerable<MarkupVisitor> _visitors;

    //    public MultiVisitor(IEnumerable<MarkupVisitor> visitors)
    //    {
    //        _visitors = visitors;
    //    }

    //    public override IMarkupElement Visit(IMarkupElement element)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    protected override string Element(IEnumerable<string> selectors)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    protected override string Text(string text)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}