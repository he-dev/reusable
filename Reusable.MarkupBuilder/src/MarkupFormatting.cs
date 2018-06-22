using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable.MarkupBuilder
{
    public interface IMarkupFormatting : IDictionary<string, MarkupFormattingOptions>
    {
        int IndentWidth { get; }
    }

    [PublicAPI]
    public abstract class MarkupFormatting : Dictionary<string, MarkupFormattingOptions>, IMarkupFormatting
    {
        protected MarkupFormatting(IEqualityComparer<string> comparer) : base(comparer)
        {
        }

        public int IndentWidth { get; set; } = 4;

        //        public MarkupFormattingOptions this[T tag]
        //        {
        //            get => _options.TryGetValue(tag, out var tagFormattingOptions) ? tagFormattingOptions : MarkupFormattingOptions.None;
        //            set => _options[tag] = value;
        //        }
    }

    public class HtmlFormatting : MarkupFormatting
    {
        public HtmlFormatting() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public static readonly HtmlFormatting Empty = new HtmlFormatting();

        public static HtmlFormatting Parse([NotNull] string template)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));

            var templateRules = MarkupFormattingTemplate.Parse(template);
            return new HtmlFormatting().AddRangeSafely(templateRules.Select(x => (x.Key, x.Value)));
        }

        public static HtmlFormatting Load([NotNull] string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));

            var template = File.ReadAllText(fileName);
            return Parse(template);
        }
    }
}