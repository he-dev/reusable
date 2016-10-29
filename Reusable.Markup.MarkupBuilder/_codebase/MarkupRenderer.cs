using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reusable.Markup
{
    public class MarkupRenderer : IMarkupRenderer
    {
        public MarkupRenderer(MarkupFormatting markupFormatting)
        {
            MarkupFormatting = markupFormatting;
        }

        private MarkupFormatting MarkupFormatting { get; set; }

        public string Render(MarkupBuilder markupBuilder)
        {
            var content = markupBuilder.Aggregate(new StringBuilder(), (sb, next) =>
            {
                var mb = next as MarkupBuilder;
                return sb.Append(mb == null ? next : Render(mb));
            })
            .ToString();

            var hasParent = markupBuilder.Parent != null;
            var placeOpeningTagOnNewLine = hasParent && MarkupFormatting[markupBuilder.Tag].HasFlag(MarkupFormattingOptions.PlaceOpeningTagOnNewLine);
            var placeClosingTagOnNewLine = MarkupFormatting[markupBuilder.Tag].HasFlag(MarkupFormattingOptions.PlaceClosingTagOnNewLine);
            var isVoid = MarkupFormatting[markupBuilder.Tag].HasFlag(MarkupFormattingOptions.IsVoid);
            var indent = IndentString(MarkupFormatting.IndentWidth * markupBuilder.Depth);

            var html = new StringBuilder();

            if (placeOpeningTagOnNewLine)
            {
                html.AppendLine();
                html.Append(indent);
            }

            html.Append(CreateOpeningElement(markupBuilder.Tag, markupBuilder.Attributes));

            if (isVoid) { return html.ToString(); }

            html.Append(content);

            if (placeClosingTagOnNewLine)
            {
                html.AppendLine();
                html.Append(hasParent ? indent : string.Empty);
            }

            html.Append(CreateClosingElement(markupBuilder.Tag));

            return html.ToString();
        }

        private static string IndentString(int indentWidth)
        {
            return new string(' ', indentWidth);
        }

        private string CreateOpeningElement(string tag, IEnumerable<KeyValuePair<string, string>> attributes)
        {
            var attributeString = CreateAttributeString(attributes);

            var html = new StringBuilder()
                .Append("<").Append(tag)
                .Append(string.IsNullOrEmpty(attributeString) ? string.Empty : " ")
                .Append(attributeString)
                //.Append(IsVoid ? "/" : string.Empty)
                .Append(">")
                .ToString();
            return html;
        }

        private static string CreateAttributeString(IEnumerable<KeyValuePair<string, string>> attributes)
        {
            return attributes.Aggregate(
                new StringBuilder(),
                (result, kvp) => result
                    .Append(result.Length > 0 ? " " : string.Empty)
                    .AppendFormat("{0}=\"{1}\"", kvp.Key, kvp.Value)
            ).ToString();
        }

        private static string CreateClosingElement(string tag)
        {
            return
                new StringBuilder()
                .Append("</")
                .Append(tag)
                .Append(">")
                .ToString();
        }
    }
}