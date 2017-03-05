using System.Collections.Generic;

namespace Reusable.Markup
{
    public abstract class MarkupFormatting : Dictionary<string, MarkupFormattingOptions>
    {
        public new MarkupFormattingOptions this[string tag]
        {
            get => TryGetValue(tag, out MarkupFormattingOptions tagFormattingOptions) ? tagFormattingOptions : MarkupFormattingOptions.None;
            set => base[tag] = value;
        }

        public int IndentWidth { get; set; }
    }
}
