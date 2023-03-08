using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Reusable.Marbles.Lexing;

public class State<TToken> where TToken : Enum
{
    private readonly IEnumerable<ITokenMatcher> _matchers;

    public State(TToken token, params TToken[] next)
    {
        Token = token;
        Next = next;
        _matchers = GetTokenMatcherProviderOrDefault().GetTokenMatchers(token).ToList();
    }

    public TToken Token { get; }

    public IEnumerable<TToken> Next { get; }

    public Token<TToken>? Match(TokenizerContext<TToken> context)
    {
        foreach (var matcher in _matchers.Where(HasMode(context)))
        {
            if (matcher.Match(context) is {} token)
            {
                try
                {
                    return token;
                }
                finally
                {
                    if (matcher.SetMode > TokenizerModes.None)
                    {
                        context.Mode = matcher.SetMode;
                    }
                }
            }
        }

        return default;
    }

    private static Func<ITokenMatcher, bool> HasMode(TokenizerContext<TToken> context)
    {
        return matcher => (matcher.Mode & context.Mode) > TokenizerModes.None;
    }

    private static TokenMatcherProviderAttribute GetTokenMatcherProviderOrDefault()
    {
        return typeof(TToken).GetCustomAttribute<TokenMatcherProviderAttribute>() ?? new EnumTokenMatcherProviderAttribute();
    }

    public override string ToString() => $"State: '{Token}', Next: [{string.Join(", ", Next)}]";
}