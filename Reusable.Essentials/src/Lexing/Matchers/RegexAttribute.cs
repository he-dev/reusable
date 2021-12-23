using System.Linq.Custom;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable.Essentials.Lexing.Matchers;

/// <summary>
/// Matches tokes with regex. Use Groups property to specify which groups to match. By default Group 1 is used. 
/// </summary>
public class RegexAttribute : TokenMatcherAttribute
{
    private readonly Regex _regex;

    public RegexAttribute([RegexPattern] string prefixPattern) => _regex = new Regex($@"\G{prefixPattern}", RegexOptions.IgnoreCase);

    public int[] Groups { get; set; } = new[] { 1 };

    public string Separator { get; set; } = string.Empty;

    public override Token<TToken>? Match<TToken>(TokenizerContext<TToken> context)
    {
        if (_regex.Match(context.Value, context.Position) is var match && match.Success)
        {
            try
            {
                return new Token<TToken>(Groups.Join(group => match.Groups[group].Value, Separator), context.Position, match.Length, context.TokenType);
            }
            finally
            {
                context.MoveBy(match.Length);
            }
        }

        return default;
    }
}