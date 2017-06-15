using System;
using System.Linq;

namespace Reusable.Markup.Html
{
    public static class HtmlExtensions
    {
        public static IMarkupElement Style(this IMarkupElement @this, params string[] css) => @this.Attribute("style", string.Join("; ", css.Select(c => $"{c.Trim().TrimEnd(';')};")));     
    }
}