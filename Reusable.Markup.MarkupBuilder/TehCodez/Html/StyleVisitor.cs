using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable.Markup.Html
{
    public interface IMarkupVisitor
    {
        [NotNull]
        IMarkupElement Visit([NotNull] IMarkupElement element);
    }

    public class StyleVisitor : IMarkupVisitor
    {
        private readonly IDictionary<string, string> _styles;

        public StyleVisitor([NotNull] IDictionary<string, string> styles)
        {
            _styles = styles ?? throw new ArgumentNullException(nameof(styles));
        }

        public IMarkupElement Visit(IMarkupElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));

            foreach (var child in Element(element))
            {
                switch (child)
                {
                    case IMarkupElement e:
                        Element(e);
                        break;
                }
            }

            return element;
        }

        // Gets element, id and class selectors.
        private static IEnumerable<string> GetSelectors(IMarkupElement element)
        {
            yield return element.Name;

            if (element.Attributes.TryGetValue("id", out var id))
            {
                yield return $"#{id}";
            }

            if (element.Attributes.TryGetValue("class", out var classes))
            {
                foreach (var className in Regex.Split(classes, @"\s+").Select(className => className.Trim()))
                {
                    yield return $".{className}";
                }
            }
        }

        private IMarkupElement Element(IMarkupElement element)
        {
            var selectors = GetSelectors(element).Distinct(StringComparer.OrdinalIgnoreCase);
            var style = GetStyles(selectors);

            if (string.IsNullOrEmpty(style))
            {
                element.Attributes.Remove("style");
            }
            else
            {
                element.Attributes["style"] = style;
            }

            return element;
        }

        private string GetStyles(IEnumerable<string> selectors)
        {
            var styles = new StringBuilder();
            foreach (var selector in selectors)
            {
                if (_styles.TryGetValue(selector, out var style))
                {
                    // Fix the ";" but trim it first in case there is already one to avoid an "if".
                    styles
                        .Append(style.Trim().TrimEnd(';'))
                        .Append(";");
                }
            }
            return styles.ToString();
        }
    }

    //public abstract class MarkupVisitor : IMarkupVisitor
    //{
    //    public abstract IMarkupElement Visit(IMarkupElement element);

    //    protected abstract string Element(IEnumerable<string> selectors);
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