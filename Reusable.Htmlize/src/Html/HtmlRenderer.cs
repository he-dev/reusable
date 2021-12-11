using System;
using System.Globalization;
using JetBrains.Annotations;

namespace Reusable.Htmlize.Html
{
    public class HtmlRenderer : MarkupRenderer
    {
        public HtmlRenderer([NotNull] IMarkupFormatting formatting, [NotNull] ISanitizer sanitizer, [NotNull] IFormatProvider formatProvider)
            : base(formatting, sanitizer, formatProvider)
        {
        }

        public HtmlRenderer(IMarkupFormatting formatting)
            : this(formatting, new HtmlSanitizer(), CultureInfo.InvariantCulture)
        {
        }
    }
}