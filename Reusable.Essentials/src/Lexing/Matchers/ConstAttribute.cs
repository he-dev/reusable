using System.Linq;

namespace Reusable.Essentials.Lexing.Matchers;

public class ConstAttribute : TokenMatcherAttribute
{
    // Can recognize constant patterns.

    private readonly string _pattern;

    public ConstAttribute(string pattern) => _pattern = pattern;

    public override Token<TToken>? Match<TToken>(TokenizerContext<TToken> context)
    {
        return
            // All characters have to be matched.
            MatchLength() == _pattern.Length
                ? new Token<TToken>(_pattern, context.Position, _pattern.Length, context.TokenType)
                : default;

        int MatchLength() => _pattern.TakeWhile((t, i) => context.Value[context.Position + i].Equals(t)).Count();
    }
}