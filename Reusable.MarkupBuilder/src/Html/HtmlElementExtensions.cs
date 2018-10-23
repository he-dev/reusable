using System;
using System.Globalization;
using System.Linq;
using System.Linq.Custom;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.MarkupBuilder.Formatters;

// ReSharper disable InconsistentNaming - lower-case method names are intentional because it's how the look in html

namespace Reusable.MarkupBuilder.Html
{
    public static class HtmlElementExtensions
    {
        public static string ToHtml<T>(this T element, [NotNull] HtmlFormatting formatting) where T : IHtmlElement
        {
            return element.ToString(default, new HtmlFormatProvider(new HtmlRenderer(formatting)));
        }

        #region Html tags

        public static T p<T>(this T element) where T : class, IHtmlElement
        {
            return element.Element("p");
        }

        public static T span<T>(this T element, params Func<T, object>[] content) where T : class, IHtmlElement
        {
            return element.Element(
                name: "span",
                local: new
                {
                    content
                },
                body: (span, local) =>
                {
                    // Exclude console template because they have already been added.
                    var items = local.content.Select(factory => factory(span)).Skip(item => item is IHtmlElement);
                    foreach (var item in items)
                    {
                        span.Add(item);
                    }
                });
        }

        #endregion

        public static T text<T>(this T template, string text) where T : class, IHtmlElement
        {
            return template.Append(text);
        }

        //public static string ToHtml(this IMarkupElement element, [NotNull] IMarkupFormatting formatting, [NotNull, ItemNotNull] IEnumerable<IMarkupModifier> visitors)
        //{
        //    return element.ToString(HtmlFormat.Html, new HtmlFormatter(new HtmlRenderer(formatting, visitors)));
        //}

        #region Attributes

        public static T @class<T>(this T element, params string[] names) where T : IHtmlElement
        {
            return element.Next(e => e.Attributes["class"] = string.Join(" ", names));
        }

        public static T id<T>(this T element, string id) where T : IHtmlElement
        {
            return element.Next(e => e.Attributes["id"] = id);
        }

        public static T data<T>(this T element, string name, string value) where T : IHtmlElement
        {
            return element.Next(e => e.Attributes[$"data-{name}"] = value);
        }

        #endregion

        #region CSS

        public static T style<T>(this T element, params (string property, string value)[] declarations) where T : class, IHtmlElement
        {
            var style = element.Attributes.TryGetValue("style", out var currentStyle) ? currentStyle : string.Empty;

            element.Attributes["style"] =
                style
                    .ToDeclarations()
                    .AddOrUpdate(declarations)
                    .ToStyle();

            return element;
        }

        //        public static T style<T>(this T element, params string[] css) where T : IHtmlElement
        //        {
        //            return element.Then(e => e.Attributes["style"] = css.Where(x => x.IsNotNullOrEmpty()).Select(x => x.Trim(';') + ";").Join(" "));
        //        }

        public static T color<T>(this T element, Enum color) where T : class, IHtmlElement
        {
            return element.color(color.ToString());
        }

        public static T color<T>(this T element, string color) where T : class, IHtmlElement
        {
            return element.style(("color", color));
        }

        public static T backgroundColor<T>(this T element, Enum color) where T : class, IHtmlElement
        {
            return element.backgroundColor(color.ToString());
        }

        public static T backgroundColor<T>(this T element, string color) where T : class, IHtmlElement
        {
            return element.style(("background-color", color));
        }

        #endregion
    }
}