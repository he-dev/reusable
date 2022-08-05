using System;
using System.Linq;
using System.Linq.Custom;
using Reusable.Fluorite.Html;
using Reusable.Marbles;

// ReSharper disable InconsistentNaming - lower-case method names are intentional because it's how the look in html

namespace Reusable.Fluorite;

public static class HtmlElementExtensions
{

    //public static string ToHtml(this IMarkupElement element, [NotNull] IMarkupFormatting formatting, [NotNull, ItemNotNull] IEnumerable<IMarkupModifier> visitors)
    //{
    //    return element.ToString(HtmlFormat.Html, new HtmlFormatter(new HtmlRenderer(formatting, visitors)));
    //}

    #region Attributes

    public static IHtmlElement attr(this IHtmlElement parent, string name, string value)
    {
        return parent.Also(e => e.Attributes[name] = value);
    }

    public static IHtmlElement @class(this IHtmlElement parent, params string[] names)
    {
        return parent.attr("class", names.Join(" "));
    }

    public static IHtmlElement id(this IHtmlElement element, string id)
    {
        return element.attr("id", id);
    }

    public static IHtmlElement data(this IHtmlElement element, string name, string value)
    {
        return element.attr($"data-{name}", value);
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