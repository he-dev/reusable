using System;

namespace Reusable.MarkupBuilder.Formatters
{
    public class HtmlFormatter : CustomFormatter
    {
        private readonly IMarkupRenderer _renderer;

        public HtmlFormatter(IMarkupRenderer renderer)
        {
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        public override object GetFormat(Type formatType)
        {
            return formatType == typeof(IMarkupElement) ? this : null;
        }

        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            return
                format.Equals(HtmlFormat.Html, StringComparison.OrdinalIgnoreCase) && (arg is IMarkupElement root)
                    ? _renderer.Render(root)
                    : ToString();
        }
    }

    public static class HtmlFormat
    {
        public const string Html = "Html";
    }
}