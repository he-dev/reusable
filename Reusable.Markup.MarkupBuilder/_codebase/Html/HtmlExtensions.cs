using System;
using System.Linq;

namespace Reusable.Markup
{
    public static class HtmlExtensions
    {
        public static IMarkupElement h1(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(h1), content);
        public static IMarkupElement h2(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(h2), content);
        public static IMarkupElement h3(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(h3), content);
        public static IMarkupElement h4(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(h4), content);
        public static IMarkupElement h5(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(h5), content);
        public static IMarkupElement h6(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(h6), content);

        public static IMarkupElement p(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(p), content);

        public static IMarkupElement div(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(div), content);
        public static IMarkupElement span(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(span), content);

        public static IMarkupElement table(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(table), content);
        public static IMarkupElement thead(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(thead), content);
        public static IMarkupElement tbody(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(tbody), content);
        public static IMarkupElement tfoot(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(tfoot), content);
        public static IMarkupElement th(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(th), content);
        public static IMarkupElement tr(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(tr), content);
        public static IMarkupElement td(this IMarkupElement @this, params object[] content) => @this.createElement(nameof(td), content);

        public static IMarkupElement id(this IMarkupElement @this, string id) => @this.attr("id", id);
        public static IMarkupElement style(this IMarkupElement @this, params string[] css) => @this.attr("style", string.Join("; ", css.Select(c => $"{c.Trim().TrimEnd(';')};")));

        #region Delegate extensions

        public static IMarkupElement h1(this IMarkupElement @this, params Func<IMarkupElement, object>[] content) => @this.createElement(nameof(h1), content);
        public static IMarkupElement h2(this IMarkupElement @this, params Func<IMarkupElement, object>[] content) => @this.createElement(nameof(h2), content);
        public static IMarkupElement h3(this IMarkupElement @this, params Func<IMarkupElement, object>[] content) => @this.createElement(nameof(h3), content);
        public static IMarkupElement h4(this IMarkupElement @this, params Func<IMarkupElement, object>[] content) => @this.createElement(nameof(h4), content);
        public static IMarkupElement h5(this IMarkupElement @this, params Func<IMarkupElement, object>[] content) => @this.createElement(nameof(h5), content);
        public static IMarkupElement h6(this IMarkupElement @this, params Func<IMarkupElement, object>[] content) => @this.createElement(nameof(h6), content);

        public static IMarkupElement p(this IMarkupElement @this, params Func<IMarkupElement, object>[] content) => @this.createElement(nameof(p), content);

        public static IMarkupElement div(this IMarkupElement @this, params Func<IMarkupElement, object>[] content) => @this.createElement(nameof(div), content);
        public static IMarkupElement span(this IMarkupElement @this, params Func<IMarkupElement, object>[] content) => @this.createElement(nameof(span), content);

        public static IMarkupElement table(this IMarkupElement @this, params Func<IMarkupElement, object>[] content) => @this.createElement(nameof(table), content);
        public static IMarkupElement thead(this IMarkupElement @this, params Func<IMarkupElement, object>[] content) => @this.createElement(nameof(thead), content);
        public static IMarkupElement tbody(this IMarkupElement @this, params Func<IMarkupElement, object>[] content) => @this.createElement(nameof(tbody), content);
        public static IMarkupElement tfoot(this IMarkupElement @this, params Func<IMarkupElement, object>[] content) => @this.createElement(nameof(tfoot), content);
        public static IMarkupElement th(this IMarkupElement @this, params Func<IMarkupElement, object>[] content) => @this.createElement(nameof(th), content);
        public static IMarkupElement tr(this IMarkupElement @this, params Func<IMarkupElement, object>[] content) => @this.createElement(nameof(tr), content);
        public static IMarkupElement td(this IMarkupElement @this, params Func<IMarkupElement, object>[] content) => @this.createElement(nameof(td), content);

        #endregion
    }
}