using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.Markup.Formatters;

namespace Reusable.Markup.Html
{
    public static class MarkupElementExtensions
    {
        private static readonly HtmlFormatting Formatting = new HtmlFormatting();

        public static string ToHtml(this IMarkupElement @this, HtmlFormatting formatting = null) => @this.ToString("html", new HtmlFormatter(new HtmlRenderer(formatting ?? Formatting)));
    }
}
