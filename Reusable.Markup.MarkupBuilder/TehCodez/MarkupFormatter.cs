using System;
using Reusable.StringFormatting;

namespace Reusable.Markup
{
    public class MarkupFormatter : CustomFormatter
    {
        private readonly IMarkupRenderer _renderer;

        public MarkupFormatter(IMarkupRenderer renderer)
        {
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        public override object GetFormat(Type formatType)
        {
            return formatType == typeof(MarkupFormatter) ? this : null;
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