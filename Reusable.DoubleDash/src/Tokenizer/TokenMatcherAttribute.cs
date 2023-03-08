using System;
using System.Collections.Generic;
using System.Reflection;

namespace Reusable.Marbles.Lexing;

public interface ITokenMatcher
{
    int WhenMode { get; }
        
    int BeginMode { get; }

    Token<TToken>? Match<TToken>(TokenizerContext<TToken> context) where TToken : Enum;
}

public abstract class TokenMatcherProviderAttribute : Attribute
{
    public abstract IEnumerable<ITokenMatcher> GetTokenMatchers<TToken>(TToken tokenType);
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public abstract class TokenMatcherAttribute : Attribute, ITokenMatcher
{
    public int WhenMode { get; set; } = TokenizerModes.Default;

    public int BeginMode { get; set; } = TokenizerModes.None;

    public abstract Token<TToken>? Match<TToken>(TokenizerContext<TToken> context) where TToken : Enum;
}

public class EnumTokenMatcherProviderAttribute : TokenMatcherProviderAttribute
{
    public override IEnumerable<ITokenMatcher> GetTokenMatchers<TToken>(TToken tokenType)
    {
        if (!typeof(TToken).IsEnum) throw new ArgumentException($"Token must be of Enum type.");

        return
            typeof(TToken)
                .GetField(tokenType.ToString())
                .GetCustomAttributes<TokenMatcherAttribute>();
    }
}