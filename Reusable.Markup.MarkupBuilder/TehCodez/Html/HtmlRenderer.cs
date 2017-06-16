using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Markup.Html
{
    public class HtmlRenderer : IMarkupRenderer
    {
        private readonly IMarkupRenderer _renderer;
        private readonly IEnumerable<IMarkupVisitor> _visitors;

        //public HtmlRenderer([NotNull] IMarkupRenderer renderer)
        //{
        //    _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        //}

        public HtmlRenderer([NotNull] IMarkupFormatting formatting, [NotNull] ISanitizer sanitizer, [NotNull] IFormatProvider formatProvider, [NotNull] IEnumerable<IMarkupVisitor> visitors)
        {
            if (formatting == null) throw new ArgumentNullException(nameof(formatting));
            if (sanitizer == null) throw new ArgumentNullException(nameof(sanitizer));
            if (formatProvider == null) throw new ArgumentNullException(nameof(formatProvider));

            _renderer = new MarkupRenderer(formatting, sanitizer, formatProvider);
            _visitors = visitors?.ToList() ?? throw new ArgumentNullException(nameof(visitors));
        }

        //public HtmlRenderer(IMarkupFormatting formatting, ISanitizer sanitizer)
        //    : this(formatting, sanitizer, CultureInfo.InvariantCulture, Enumerable.Empty<IMarkupVisitor>())
        //{ }

        public HtmlRenderer(IMarkupFormatting formatting, IEnumerable<IMarkupVisitor> visitors)
            : this(formatting, new HtmlSanitizer(), CultureInfo.InvariantCulture, visitors)
        {
        }

        public HtmlRenderer(IMarkupFormatting formatting)
            : this(formatting, new HtmlSanitizer(), CultureInfo.InvariantCulture, Enumerable.Empty<IMarkupVisitor>())
        { }

        public string Render(IMarkupElement markupElement)
        {
            markupElement = _visitors.Aggregate(markupElement, (current, visitor) => visitor.Visit(current));
            return _renderer.Render(markupElement);
        }
    }
}