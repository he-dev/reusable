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

        public string Render(IElement element)
        {
            var content = element.Aggregate(new StringBuilder(), (sb, next) =>
            {
                var mb = next as MarkupBuilder;
                return sb.Append(mb == null ? next : Render(mb));
            })
            .ToString();

            var hasParent = element.Parent != null;
            var placeOpeningTagOnNewLine = hasParent && MarkupFormatting[element.Name].HasFlag(MarkupFormattingOptions.PlaceOpeningTagOnNewLine);
            var placeClosingTagOnNewLine = MarkupFormatting[element.Name].HasFlag(MarkupFormattingOptions.PlaceClosingTagOnNewLine);
            var isVoid = MarkupFormatting[element.Name].HasFlag(MarkupFormattingOptions.IsVoid);
            var indent = IndentString(MarkupFormatting.IndentWidth * CalcDepth(element));

            var html = new StringBuilder();

            if (placeOpeningTagOnNewLine)
            {
                html.AppendLine();
                html.Append(indent);
            }

            html.Append(CreateOpeningElement(element.Name, element.Attributes));

            if (isVoid) { return html.ToString(); }

            html.Append(content);

            if (placeClosingTagOnNewLine)
            {
                html.AppendLine();
                html.Append(hasParent ? indent : string.Empty);
            }

            html.Append(CreateClosingElement(element.Name));

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

        internal int CalcDepth(IElement element)
        {
            var depth = 0;
            var parent = element.Parent;
            while (parent != null)
            {
                depth++;
                parent = parent.Parent;
            }
            return depth;
        }
    }
}