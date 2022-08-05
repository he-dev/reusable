using System;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Marbles.Extensions;

namespace Reusable.Marbles.Drawing;

[UsedImplicitly]
public abstract class ColorParser
{
    protected const int Empty = 0;

    [ContractAnnotation("value: null => halt")]
    public int Parse(string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (TryParse(value, out var result))
        {
            return result;
        }

        throw new FormatException($"Unknown color format: '{value}'");
    }

    [ContractAnnotation("value: null => false")]
    public bool TryParse(string? value, out int color)
    {
        if (value is { })
        {
            return TryParseCore(value, out color);
        }

        color = Empty;
        return false;
    }

    protected abstract bool TryParseCore(string value, out int color);
}

public class RgbColorParser : ColorParser
{
    // language=regexp
    private const string ColorPattern = @"\d{1,2}|[1][0-9][0-9]|[2][0-5][0-5]";

    // language=regexp
    private const string DelimiterPattern = @"[,;:]";

    // language=regexp
    private const string ArgbPattern =
        // Using $ everywhere for consistency.
        $"^(?:" +
        $"(?<A>{ColorPattern}){DelimiterPattern})?" +
        $"(?<R>{ColorPattern}){DelimiterPattern}" +
        $"(?<G>{ColorPattern}){DelimiterPattern}" +
        $"(?<B>{ColorPattern}" +
        $")$";

    protected override bool TryParseCore(string? value, out int color)
    {
        var match = Regex.Match(value.Minify()!, ArgbPattern, RegexOptions.ExplicitCapture);
        if (match.Success)
        {
            var alpha = match.Groups["A"].Success ? int.Parse(match.Groups["A"].Value) : 255;
            var red = int.Parse(match.Groups["R"].Value);
            var green = int.Parse(match.Groups["G"].Value);
            var blue = int.Parse(match.Groups["B"].Value);

            color = Color.FromArgb(alpha, red, green, blue).ToArgb();
            return true;
        }

        color = Empty;
        return false;
    }
}

public class HexColorParser : ColorParser
{
    // language=regexp
    private const string ColorPattern = @"^(0x|#)?(?<A>[A-F0-9]{2})?(?<R>[A-F0-9]{2})(?<G>[A-F0-9]{2})(?<B>[A-F0-9]{2})$";

    protected override bool TryParseCore(string value, out int color)
    {
        if (value.Minify()!.TryMatch(ColorPattern, out var match, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture))
        {
            var alpha = match.Groups["A"].Success ? int.Parse(match.Groups["A"].Value, NumberStyles.HexNumber) : 255;
            var red = int.Parse(match.Groups["R"].Value, NumberStyles.HexNumber);
            var green = int.Parse(match.Groups["G"].Value, NumberStyles.HexNumber);
            var blue = int.Parse(match.Groups["B"].Value, NumberStyles.HexNumber);

            color = Color.FromArgb(alpha, red, green, blue).ToArgb();
            return true;
        }

        color = Empty;
        return false;
    }
}

public class NameColorParser : ColorParser
{
    protected override bool TryParseCore(string value, out int color)
    {
        var colorFromName = Color.FromName(value.Minify()!);

        if (colorFromName.IsKnownColor)
        {
            color = colorFromName.ToArgb();
            return true;
        }

        color = Empty;
        return false;
    }
}