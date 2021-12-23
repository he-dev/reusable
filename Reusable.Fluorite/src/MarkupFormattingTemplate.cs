using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.Essentials.Extensions;

namespace Reusable.Fluorite;
// Maybe another time...
//public interface IMarkupFormattingTemplateParser
//{
//    IDictionary<string, MarkupFormattingOptions> Parse([NotNull] string template);
//}

public static class MarkupFormattingTemplate
{
    public static IDictionary<string, MarkupFormattingOptions> Parse(string template)
    {
        if (template == null) throw new ArgumentNullException(nameof(template));

        var tags =
            template
                .SplitByLineBreaks()
                .Parse()
                .ToList();

        var openingTagOptions = tags.DetermineOpeningTagOptions();
        var closingTagOptions = tags.DetermineClosingTagOptions();

        return Merge(openingTagOptions, closingTagOptions);
    }

    private static IEnumerable<Tag> Parse(this IEnumerable<string> lines)
    {
        return
            lines
                .Select((line, lineNumber) =>
                    ParseLine(line)
                        .Select(m => new Tag
                        {
                            Name = m.Groups["name"].Value.ToLowerInvariant(),
                            Line = lineNumber,
                            Column = m.Groups["name"].Index
                        }))
                .SelectMany(x => x);

        IEnumerable<Match> ParseLine(string line)
        {
            return
                Regex
                    // This pattern matches any tag name (opening or closing).
                    .Matches(line, @"</?(?<name>[a-z0-9]+)>", RegexOptions.ExplicitCapture)
                    .Cast<Match>();
        }
    }

    private static IEnumerable<KeyValuePair<string, MarkupFormattingOptions>> DetermineClosingTagOptions(this ICollection<Tag> tags)
    {
        // Group elements by name to first find out how many lines they take.
        foreach (var nameGroup in tags.GroupBy(t => t.Name))
        {
            // If any tag is found in more the one line then the closing tag should be placed on a new line.
            var newLineOption =
                nameGroup.Select(i => i.Line).Distinct().Count() > 1
                    ? MarkupFormattingOptions.PlaceClosingTagOnNewLine
                    : MarkupFormattingOptions.None;

            // If any tag occurs only once then it's void.
            var voidOption =
                nameGroup.Count() == 1
                    ? MarkupFormattingOptions.IsVoid
                    : MarkupFormattingOptions.None;

            yield return new KeyValuePair<string, MarkupFormattingOptions>(nameGroup.Key, newLineOption | voidOption);
        }
    }

    private static IEnumerable<KeyValuePair<string, MarkupFormattingOptions>> DetermineOpeningTagOptions(this ICollection<Tag> tags)
    {
        // To find the new-line-option for the opening tag 
        // we need to evaluate each line to see if the current tag has any predecessors
        // so we first group tags by line. 
        // A line can contain more then one tag 
        // but we are interest on in the first one that has the current name.
        // If the index of the name is 0 then this tag is first in this line and thus needs the new-line-option.

        foreach (var tagName in tags.Select(t => t.Name).Distinct())
        {
            var newLineOption =
                tags
                    .GroupBy(t => t.Line)
                    .First(g => g.Any(x => x.Name == tagName))
                    .Select((item, index) => new {item, index})
                    .First(x => x.item.Name == tagName).index == 0
                    ? MarkupFormattingOptions.PlaceOpeningTagOnNewLine
                    : MarkupFormattingOptions.None;

            yield return new KeyValuePair<string, MarkupFormattingOptions>(tagName, newLineOption);
        }
    }

    private static IDictionary<string, MarkupFormattingOptions> Merge(
        IEnumerable<KeyValuePair<string, MarkupFormattingOptions>> options1,
        IEnumerable<KeyValuePair<string, MarkupFormattingOptions>> options2)
    {
        var result = options1.ToDictionary(x => x.Key, x => x.Value);

        foreach (var item in options2)
        {
            result[item.Key] |= item.Value;
        }

        return result;
    }

    private class Tag
    {
        public string Name { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
    }
}