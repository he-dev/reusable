using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Markup.Formatters;

namespace Reusable.Markup.Html
{
    public static class MarkupElementExtensions
    {
        public static string ToHtml(this IMarkupElement element, [NotNull] IMarkupFormatting formatting)
        {
            return element.ToString(HtmlFormat.Html, new HtmlFormatter(new HtmlRenderer(formatting)));
        }

        //public static string ToHtml(this IMarkupElement element, [NotNull] IMarkupFormatting formatting, [NotNull, ItemNotNull] IEnumerable<IMarkupModifier> visitors)
        //{
        //    return element.ToString(HtmlFormat.Html, new HtmlFormatter(new HtmlRenderer(formatting, visitors)));
        //}

        public static IMarkupElement Class(this IMarkupElement element, params string[] names)
        {
            return element.Tee(e => e.Attributes["class"] = string.Join(" ", names));
        }

        public static IMarkupElement Id(this IMarkupElement element, string id)
        {
            return element.Tee(e => e.Attributes["id"] = id);
        }

        public static IMarkupElement Data(this IMarkupElement element, string name, string value)
        {
            return element.Tee(e => e.Attributes[$"data-{name}"] = value);
        }
    }
}
