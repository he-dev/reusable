using System;
using System.Collections.Generic;

namespace Reusable.Marbles.Lexing;

public interface ITokenizer<TToken> where TToken : Enum
{
    IEnumerable<Token<TToken>> Tokenize(string? value);
}

public class Tokenizer<TToken> : ITokenizer<TToken> where TToken : Enum
{
    private readonly StateTransition<TToken> _stateTransition;

    public Tokenizer(StateTransition<TToken> stateTransition) => _stateTransition = stateTransition;

    public IEnumerable<Token<TToken>> Tokenize(string? value)
    {
        var context = new TokenizerContext<TToken> { Value = (value ?? string.Empty).Trim() };

        while (!context.Eof)
        {
            foreach (var next in _stateTransition.Next(context.TokenType))
            {
                context.TokenType = next.Token;
                if (next.Match(context) is {} token)
                {
                    yield return token;
                    goto next;
                }
            }

            //throw DynamicException.Create("UnknownToken", $"Could not parse token '{context.Current}' at {context.Position}.");
            throw new Exception();

            next: ;
        }
    }
}

public static class TokenizerModes
{
    public const int None = 0;

    public const int Default = 1;
}