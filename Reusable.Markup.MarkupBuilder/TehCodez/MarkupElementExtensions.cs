using System;
using Reusable.Extensions;
// ReSharper disable InconsistentNaming

namespace Reusable.Markup
{
    public static class MarkupElementExtensions
    {
        public static IMarkupElement createElement(this IMarkupElement @this, string name, params object[] content)
        {
            // If content is null then use a dummy to output an empty string.
            var element = new MarkupElement(name, content ?? new object[0]);
            @this?.Add(element);
            return element;
        }

        public static IMarkupElement createElement(this IMarkupElement @this, string name, params Func<IMarkupElement, object>[] content)
        {
            // If content is null then use a dummy to output an empty string.
            var element = new MarkupElement(name, content ?? new Func<IMarkupElement, object>[0]);
            @this?.Add(element);
            return element;
        }

        public static IMarkupElement attr(this IMarkupElement @this, string name, string value) => @this.Tee(e => e.Attributes[name] = value);
    }
}