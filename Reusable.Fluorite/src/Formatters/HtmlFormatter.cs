using System;
using Reusable.Marbles.FormatProviders;

namespace Reusable.Fluorite.Formatters;

public class HtmlFormatProvider : CustomFormatProvider
{
    public HtmlFormatProvider(IMarkupRenderer markupRenderer) : base(new HtmlFormatter(markupRenderer)) { }

    private class HtmlFormatter : ICustomFormatter
    {
        private readonly IMarkupRenderer _renderer;

        public HtmlFormatter(IMarkupRenderer renderer) => _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (!(format is null)) throw new ArgumentNullException($"'{nameof(HtmlFormatter)}' does not support formats.");
            if (arg is null) return string.Empty;

            return
                arg is IHtmlElement markupElement
                    ? _renderer.Render(markupElement)
                    : null;
        }
    }
}

public static class HtmlFormat
{
    public const string Html = "Html";
}