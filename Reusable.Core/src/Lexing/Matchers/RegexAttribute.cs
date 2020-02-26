using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable.Lexing.Matchers
{
    public class RegexAttribute : TokenMatcherAttribute
    {
        // Can recognize regexable patterns.
        // The pattern requires one group that is the token to return.

        private readonly Regex _regex;

        public RegexAttribute([RegexPattern] string prefixPattern) => _regex = new Regex($@"\G{prefixPattern}", RegexOptions.IgnoreCase);

        public override Token<TToken>? Match<TToken>(TokenizerContext<TToken> context)
        {
            if (_regex.Match(context.Value, context.Position) is var match && match.Success)
            {
                try
                {
                    return new Token<TToken>(match.Groups[1].Value, context.Position, match.Length, context.TokenType);
                }
                finally
                {
                    context.MoveBy(match.Length);
                }
            }

            return default;
        }
    }
}