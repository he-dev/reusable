using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Extensions;
// ReSharper disable InconsistentNaming

namespace Reusable.Markup
{
    public static class MarkupElementExtensions
    {
        public static IMarkupElement Element(this IMarkupElement @this, string name)
        {
            var element = MarkupElement.Create(name, @this);
            return @this ?? element;
        }

        public static IMarkupElement Element(this IMarkupElement @this, string name, Action<IMarkupElement> configureElement)
        {
            var element = MarkupElement.Create(name, @this);
            configureElement(element);
            return @this ?? element;
        }

        public static IMarkupElement Element(this IMarkupElement @this, string name, IEnumerable<object> content)
        {
            var element = MarkupElement.Create(name, @this).Append(content);
            return @this ?? element;
        }

        public static IMarkupElement Element(this IMarkupElement @this, string name, Func<IMarkupElement, IEnumerable<IMarkupElement>> createContent)
        {
            var element = MarkupElement.Create(name, @this).Append(createContent(MarkupElement.Builder));
            return @this ?? element;
        }

        public static IMarkupElement Element(this IMarkupElement @this, string name, params object[] content)
        {
            var element = MarkupElement.Create(name, @this).Append(content ?? new object[0]);
            return @this ?? element;
        }

        public static IMarkupElement Elements<T>(this IMarkupElement @this, string name, IEnumerable<T> content, Action<IMarkupElement, T, int> configureElement)
        {
            if (@this.IsNull()) { throw new ArgumentNullException(paramName: nameof(@this), message: "You cannot add elements to a null element (or builder)."); }

            var index = 0;
            foreach (var item in content)
            {
                var element = MarkupElement.Create(name);
                @this.Add(element);
                configureElement?.Invoke(element, item, index++);
            }
            return @this;
        }

        public static IMarkupElement Elements<T>(this IMarkupElement @this, string name, IEnumerable<T> content, Action<IMarkupElement, T> configureElement)
        {
            return @this.Elements(name, content, (current, x, i) => configureElement(current, x));
        }

        public static IMarkupElement Append(this IMarkupElement @this, object content)
        {
            return @this.Tee(element => element.Add(content));
        }

        public static IMarkupElement Append(this IMarkupElement @this, IEnumerable<object> items)
        {
            return @this.Tee(element => { foreach (var item in items) @this.Append(item); });
        }

        public static IMarkupElement Attribute(this IMarkupElement @this, string name, string value)
        {
            return @this.Tee(e => e.Attributes[name] = value);
        }
    }
}