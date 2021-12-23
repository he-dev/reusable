using System.Linq;
using System.Linq.Custom;

namespace Reusable.Fluorite.Html;

public static class HtmlExtensions
{
    //public static HtmlElement Style(this HtmlElement @this, params string[] css) => (HtmlElement)@this.Attribute("style", string.Join("; ", css.Select(c => $"{c.Trim().TrimEnd(';')};")));     
    
    public static HtmlElement Style(this HtmlElement @this, params string[] css) => (HtmlElement)@this.Attribute("style", css.Select(c => c.Trim().TrimEnd(';') + ";").Join("; "));     
}