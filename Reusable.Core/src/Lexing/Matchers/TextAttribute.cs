using System.Collections.Immutable;
using System.Text;

namespace Reusable.Lexing.Matchers
{
    public class TextAttribute : TokenMatcherAttribute
    {
        // Assists regex in tokenizing quoted strings because regex has no memory of what it has seen.
        // Requires two patterns:
        // - one for the separator because it has to know where the value begins
        // - the other for an unquoted value if it's not already quoted

        public static readonly IImmutableSet<char> Escapables = new[] { '\\', '"', '\'', '-' }.ToImmutableHashSet();

        public override Token<TToken>? Match<TToken>(TokenizerContext<TToken> context)
        {
            var length = 0;

            // Skip whitespace.
            while (context.Current == ' ' && context.MoveNext())
            {
                length++;
            }

            var cache = new StringBuilder();
            var startsAt = context.Position;
            var escapeSequence = false;
            var quote = char.MinValue; // Opening/closing quote.

            do
            {
                if (quote == char.MinValue)
                {
                    if (@"'""".IndexOf(context.Current) >= 0)
                    {
                        quote = context.Current;
                    }
                    else
                    {
                        try
                        {
                            // It doesn't start with a quote.
                            return default;
                        }
                        finally
                        {
                            // backtrack
                            context.Backtrack(length);
                        }
                    }
                }
                else
                {
                    if (context.Current == '\\' && !escapeSequence)
                    {
                        escapeSequence = true;
                    }
                    else
                    {
                        if (escapeSequence)
                        {
                            if (Escapables.Contains(context.Current))
                            {
                                // Remove escape char. We don't need them in the result.
                                cache.Length--;
                            }

                            escapeSequence = false;
                        }
                        else
                        {
                            if (context.Current == quote)
                            {
                                // + the last quote
                                try
                                {
                                    return new Token<TToken>(cache.ToString(), startsAt, length + 1, context.TokenType);
                                }
                                finally
                                {
                                    context.MoveBy(1);
                                }
                            }
                        }
                    }

                    cache.Append(context.Current);
                }
            } while (context.MoveNext());

            return default;
        }
    }
}