using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reusable.Markup
{
    public interface IMarkupRenderer
    {
        string Render(IElement element);
    }

    public class MarkupRenderer : IMarkupRenderer
    {
        public MarkupRenderer(MarkupFormatting markupFormatting)
        {
            MarkupFormatting = markupFormatting ?? throw new ArgumentNullException(nameof(markupFormatting));
        }

        private MarkupFormatting MarkupFormatting { get; set; }

        public string Render(IElement element)
        {
            var content = element.Aggregate(
                new StringBuilder(),
                (result, next) => result.Append(next is IElement e ? Render(e) : next)
            )
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

            if (!isVoid)
            {
                html.Append(content);

                if (placeClosingTagOnNewLine)
                {
                    html.AppendLine();
                    html.Append(hasParent ? indent : string.Empty);
                }

                html.Append(CreateClosingElement(element.Name));
            }

            return html.ToString();
        }

        private static string IndentString(int indentWidth) => new string(' ', indentWidth);

        private string CreateOpeningElement(string tag, IEnumerable<KeyValuePair<string, string>> attributes)
        {
            var attributeString = CreateAttributeString(attributes);
            attributeString = string.IsNullOrEmpty(attributeString) ? string.Empty : $" {attributeString}";
            return $"<{tag}{attributeString}>";
        }

        private static string CreateAttributeString(IEnumerable<KeyValuePair<string, string>> attributes)
        {
            return attributes.Aggregate(
                new StringBuilder(),
                (result, next) => result
                    .Append(result.Length > 0 ? " " : string.Empty)
                    .Append($"{next.Key}=\"{next.Value}\"")
            ).ToString();
        }

        private static string CreateClosingElement(string tag)
        {
            return $"</{tag}>";
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