using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Reusable.Extensions;

namespace Reusable.Markup
{
    public interface IMarkupRenderer
    {
        string Render(IMarkupElement markupElement);
    }

    public abstract class MarkupRenderer : IMarkupRenderer
    {
        private readonly IMarkupFormatting _formatting;
        private readonly ISanitizer _sanitizer;
        private readonly IFormatProvider _formatProvider;

        protected MarkupRenderer(IMarkupFormatting formatting, ISanitizer sanitizer, IFormatProvider formatProvider)
        {
            _formatting = formatting ?? throw new ArgumentNullException(nameof(formatting));
            _sanitizer = sanitizer ?? throw new ArgumentNullException(nameof(sanitizer));
            _formatProvider = formatProvider ?? throw new ArgumentNullException(nameof(formatProvider));
        }

        protected MarkupRenderer(IMarkupFormatting formatting, ISanitizer sanitizer)
            : this(formatting, sanitizer, CultureInfo.InvariantCulture)
        { }

        #region IMarkupRenderer

        public string Render(IMarkupElement markupElement)
        {
            var content = (markupElement ?? throw new ArgumentNullException(nameof(markupElement))).Aggregate(
                    new StringBuilder(),
                    (result, next) => result.Append(
                        next is IMarkupElement e
                            ? Render(e)
                            : _sanitizer.Sanitize(next, _formatProvider)))
                .ToString();

            var indent = markupElement.Parent != null;
            var placeOpeningTagOnNewLine = _formatting[markupElement.Name].HasFlag(MarkupFormattingOptions.PlaceOpeningTagOnNewLine) && markupElement.Parent.IsNotNull();
            var placeClosingTagOnNewLine = _formatting[markupElement.Name].HasFlag(MarkupFormattingOptions.PlaceClosingTagOnNewLine);
            var hasClosingTag = _formatting[markupElement.Name].HasFlag(MarkupFormattingOptions.IsVoid) == false;
            var indentString = IndentString(_formatting.IndentWidth, markupElement.Depth);

            var html =
                new StringBuilder()
                    .Append(IndentTag(placeOpeningTagOnNewLine, indent, indentString))
                    .Append(RenderOpeningTag(markupElement.Name, markupElement.Attributes))
                    .AppendWhen(() => hasClosingTag, sb => sb
                        .Append(content)
                        .Append(IndentTag(placeClosingTagOnNewLine, indent, indentString))
                        .Append(RenderClosingTag(markupElement.Name)));

            return html.ToString();
        }

        #endregion

        private static string IndentTag(bool newLine, bool indent, string indentString)
        {
            return
                new StringBuilder()
                    .AppendWhen(() => newLine, sb => sb.AppendLine())
                    .AppendWhen(() => newLine && indent, sb => sb.Append(indentString))
                    .ToString();
        }

        private static string RenderOpeningTag(string tag, IEnumerable<KeyValuePair<string, string>> attributes)
        {
            return
                new StringBuilder()
                    .Append($"<{tag}")
                    .AppendWhen(
                        () => RenderAttributes(attributes).ToList(),
                        attributeStrings => attributeStrings.Any(),
                        (sb, attributeStrings) => sb.Append($" {string.Join(" ", attributeStrings)}"))
                    .Append(">").ToString();
        }

        private static IEnumerable<string> RenderAttributes(IEnumerable<KeyValuePair<string, string>> attributes)
        {
            return attributes.Select(attr => $"{attr.Key}=\"{attr.Value}\"");
        }

        private static string RenderClosingTag(string tag)
        {
            return $"</{tag}>";
        }

        private static string IndentString(int indentWidth, int depth)
        {
            return new string(' ', indentWidth * depth);
        }
    }
}