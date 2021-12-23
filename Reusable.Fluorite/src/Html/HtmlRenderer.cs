using System;
using System.Globalization;

namespace Reusable.Fluorite.Html;

public class HtmlRenderer : MarkupRenderer
{
    public HtmlRenderer(IMarkupFormatting formatting, ISanitizer sanitizer, IFormatProvider formatProvider)
        : base(formatting, sanitizer, formatProvider)
    {
    }

    public HtmlRenderer(IMarkupFormatting formatting)
        : this(formatting, new HtmlSanitizer(), CultureInfo.InvariantCulture)
    {
    }
}