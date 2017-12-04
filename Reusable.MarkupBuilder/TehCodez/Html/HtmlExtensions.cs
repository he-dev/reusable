using System.Linq;
using System.Linq.Custom;
using Reusable.Extensions;

namespace Reusable.MarkupBuilder.Html
{
    public static class HtmlExtensions
    {
        //public static HtmlElement Style(this HtmlElement @this, params string[] css) => (HtmlElement)@this.Attribute("style", string.Join("; ", css.Select(c => $"{c.Trim().TrimEnd(';')};")));     
    
        public static HtmlElement Style(this HtmlElement @this, params string[] css) => (HtmlElement)@this.Attribute("style", css.Select(c => c.Trim().TrimEnd(';') + ";").Join("; "));     
    }
}