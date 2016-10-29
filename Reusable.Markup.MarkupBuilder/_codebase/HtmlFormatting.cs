using System.Collections.Generic;

namespace Reusable.Markup
{
    public class HtmlFormatting : MarkupFormatting
    {
        public HtmlFormatting()
        {
            TagFormattingOptions = new Dictionary<string, MarkupFormattingOptions>
            {
                { "body", MarkupFormattingOptions.PlaceClosingTagOnNewLine },
                { "br", MarkupFormattingOptions.IsVoid },
                { "span", MarkupFormattingOptions.None },
                { "p", MarkupFormattingOptions.PlaceOpeningTagOnNewLine },
                { "pre", MarkupFormattingOptions.PlaceOpeningTagOnNewLine },
                { "h1", MarkupFormattingOptions.PlaceOpeningTagOnNewLine },
                { "h2", MarkupFormattingOptions.PlaceOpeningTagOnNewLine },
                { "h3", MarkupFormattingOptions.PlaceOpeningTagOnNewLine },
                { "h4", MarkupFormattingOptions.PlaceOpeningTagOnNewLine },
                { "h5", MarkupFormattingOptions.PlaceOpeningTagOnNewLine },
                { "h6", MarkupFormattingOptions.PlaceOpeningTagOnNewLine },
                { "ul", MarkupFormattingOptions.PlaceBothTagsOnNewLine },
                { "ol", MarkupFormattingOptions.PlaceBothTagsOnNewLine },
                { "li", MarkupFormattingOptions.PlaceOpeningTagOnNewLine },
                { "table", MarkupFormattingOptions.PlaceClosingTagOnNewLine },
                { "caption", MarkupFormattingOptions.PlaceOpeningTagOnNewLine },
                { "thead", MarkupFormattingOptions.PlaceBothTagsOnNewLine },
                { "tbody", MarkupFormattingOptions.PlaceBothTagsOnNewLine },
                { "tfoot", MarkupFormattingOptions.PlaceBothTagsOnNewLine },
                { "tr", MarkupFormattingOptions.PlaceBothTagsOnNewLine },
                { "th", MarkupFormattingOptions.PlaceOpeningTagOnNewLine },
                { "td", MarkupFormattingOptions.PlaceOpeningTagOnNewLine },
                { "colgroup", MarkupFormattingOptions.PlaceOpeningTagOnNewLine },
                { "col", MarkupFormattingOptions.PlaceOpeningTagOnNewLine },
            };
            IndentWidth = 4;
        }
    }
}
