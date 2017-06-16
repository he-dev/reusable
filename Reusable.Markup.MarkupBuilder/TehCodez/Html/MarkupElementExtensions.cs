using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Markup.Formatters;

namespace Reusable.Markup.Html
{
    public static class MarkupElementExtensions
    {
        public static string ToHtml(this IMarkupElement element, [NotNull] IMarkupFormatting formatting)
        {
            return element.ToString(HtmlFormat.Html, new HtmlFormatter(new HtmlRenderer(formatting)));
        }

        public static string ToHtml(this IMarkupElement element, [NotNull] IMarkupFormatting formatting, [NotNull, ItemNotNull] IEnumerable<IMarkupVisitor> visitors)
        {
            return element.ToString(HtmlFormat.Html, new HtmlFormatter(new HtmlRenderer(formatting, visitors)));
        }
    }
}
