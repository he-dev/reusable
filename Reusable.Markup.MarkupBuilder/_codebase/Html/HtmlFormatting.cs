

namespace Reusable.Markup
{

    public class HtmlFormatting : MarkupFormatting
    {
        public const int DefaultIndentWidth = 4;

        public HtmlFormatting() : this(DefaultIndentWidth)
        {
            this["body"] = MarkupFormattingOptions.PlaceClosingTagOnNewLine;
            this["br"] = MarkupFormattingOptions.IsVoid;
            //this["span"] = MarkupFormattingOptions.None;
            this["p"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
            this["pre"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
            this["h1"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
            this["h2"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
            this["h3"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
            this["h4"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
            this["h5"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
            this["h6"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
            this["ul"] = MarkupFormattingOptions.PlaceBothTagsOnNewLine;
            this["ol"] = MarkupFormattingOptions.PlaceBothTagsOnNewLine;
            this["li"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
            this["table"] = MarkupFormattingOptions.PlaceClosingTagOnNewLine;
            this["caption"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
            this["thead"] = MarkupFormattingOptions.PlaceBothTagsOnNewLine;
            this["tbody"] = MarkupFormattingOptions.PlaceBothTagsOnNewLine;
            this["tfoot"] = MarkupFormattingOptions.PlaceBothTagsOnNewLine;
            this["tr"] = MarkupFormattingOptions.PlaceBothTagsOnNewLine;
            this["th"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
            this["td"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
            this["colgroup"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
            this["col"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine;
            this["hr"] = MarkupFormattingOptions.PlaceOpeningTagOnNewLine | MarkupFormattingOptions.IsVoid;
        }

        public HtmlFormatting(int indentWidth)
        {
            IndentWidth = indentWidth;
        }
    }
}