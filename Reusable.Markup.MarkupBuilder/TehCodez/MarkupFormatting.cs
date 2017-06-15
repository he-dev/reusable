using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Markup
{
    public interface IMarkupFormatting
    {
        int IndentWidth { get; }

        MarkupFormattingOptions this[string name] { get; }
    }

    [PublicAPI]
    public class MarkupFormatting : IMarkupFormatting
    {
        private readonly IDictionary<string, MarkupFormattingOptions> _options = new Dictionary<string, MarkupFormattingOptions>(StringComparer.OrdinalIgnoreCase);

        public MarkupFormatting() { }

        public MarkupFormatting([NotNull] IEnumerable<KeyValuePair<string, MarkupFormattingOptions>> options)
        {
            //if (indentWidth < 0) { throw new ArgumentOutOfRangeException(paramName: nameof(indentWidth), message: $"{nameof(indentWidth)} must be >= 0."); }
            //IndentWidth = indentWidth;

            _options = options?.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase) ?? throw new ArgumentNullException(nameof(options));
        }

        // This never ever changes.
        public int IndentWidth { get; set; } = 4;

        public MarkupFormattingOptions this[string tag]
        {
            get => _options.TryGetValue(tag, out var tagFormattingOptions) ? tagFormattingOptions : MarkupFormattingOptions.None;
            set => _options[tag] = value;
        }

        public static IMarkupFormatting Parse(string template)
        {
            var result = MarkupFormattingTemplate.Parse(template);
            return new MarkupFormatting(result);
        }
    }
}