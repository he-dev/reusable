using System.Text;

namespace Reusable.Marbles.Lexing.Matchers;

public class TextAttribute : TokenMatcherAttribute
{
    public string Escapables { get; set; } = "\\\"'-"; 

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
                        // Let's get out of here. It doesn't start with a quote.
                        return default;
                    }
                    finally
                    {
                        // But before, rewind to where it started.
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
                        if (Escapables.IndexOf(context.Current) >= 0)
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

            length++;
        } while (context.MoveNext());

        return default;
    }
}