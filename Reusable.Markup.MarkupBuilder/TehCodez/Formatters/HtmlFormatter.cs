using System;
using Reusable.StringFormatting;

namespace Reusable.Markup.Formatters
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
            return formatType == typeof(MarkupElement) ? this : null;
        }

        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            return
                format.Equals("html", StringComparison.OrdinalIgnoreCase) && (arg is IMarkupElement root)
                    ? _renderer.Render(root)
                    : base.ToString();
        }
    }
}