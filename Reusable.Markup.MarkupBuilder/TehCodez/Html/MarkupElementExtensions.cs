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
        public static string ToHtml(this IMarkupElement @this, [NotNull] IMarkupFormatting formatting)
        {
            return @this.ToString("html", new HtmlFormatter(new HtmlRenderer(formatting)));
        }
    }
}
