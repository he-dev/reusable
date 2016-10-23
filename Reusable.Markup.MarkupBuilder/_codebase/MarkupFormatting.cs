using System.Collections.Generic;

namespace Reusable
{
    public abstract class MarkupFormatting
    {
        public MarkupFormattingOptions this[string tag]
        {
            get
            {
                var tagFormattingOptions = MarkupFormattingOptions.None;
                return
                    TagFormattingOptions.TryGetValue(tag, out tagFormattingOptions)
                    ? tagFormattingOptions
                    : MarkupFormattingOptions.None;
            }
        }

        public Dictionary<string, MarkupFormattingOptions> TagFormattingOptions { get; set; }

        public int IndentWidth { get; set; }
    }
}
