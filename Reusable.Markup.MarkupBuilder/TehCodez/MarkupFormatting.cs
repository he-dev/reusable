using System.Collections.Generic;

namespace Reusable.Markup
{
    public interface IMarkupFormatting
    {
        MarkupFormattingOptions this[string name] { get; }

        int IndentWidth { get; }
    }

    public abstract class MarkupFormatting : Dictionary<string, MarkupFormattingOptions>, IMarkupFormatting
    {
        public new MarkupFormattingOptions this[string tag]
        {
            get => TryGetValue(tag, out MarkupFormattingOptions tagFormattingOptions) ? tagFormattingOptions : MarkupFormattingOptions.None;
            set => base[tag] = value;
        }

        public int IndentWidth { get; set; }
    }
}