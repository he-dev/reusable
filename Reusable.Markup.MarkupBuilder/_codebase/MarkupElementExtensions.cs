using System;
using Reusable.Extensions;

namespace Reusable.Markup
{
    public static class MarkupElementExtensions
    {
        public static IMarkupElement createElement(this IMarkupElement @this, string name, params object[] content)
        {
            var element = new MarkupElement(name, content);
            @this?.Add(element);
            return element;
        }

        public static IMarkupElement createElement(this IMarkupElement @this, string name, params Func<IMarkupElement, object>[] content)
        {
            var element = new MarkupElement(name, content);
            @this?.Add(element);
            return element;
        }

        public static IMarkupElement attr(this IMarkupElement @this, string name, string value) => @this.Tee(e => e.Attributes[name] = value);
    }
}