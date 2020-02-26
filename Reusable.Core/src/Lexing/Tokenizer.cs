using System;
using System.Collections.Generic;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Lexing
{
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
            if (value.IsNullOrEmpty()) yield break;

            var context = new TokenizerContext<TToken>
            {
                Value = value.Trim(),
                TokenType = default,
            };

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
                
                throw DynamicException.Create("UnknownToken", $"Could not parse token at {context.Position}.");

                next: ;
            }
        }
    }
}