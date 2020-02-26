using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable.Lexing
{
    public interface ITokenizer<TToken> where TToken : Enum
    {
        IEnumerable<Token<TToken>> Tokenize(string value);
    }

    public class Tokenizer<TToken> : ITokenizer<TToken> where TToken : Enum
    {
        private readonly StateTransition<TToken> _stateTransition;

        public Tokenizer(StateTransition<TToken> stateTransition) => _stateTransition = stateTransition;

        public IEnumerable<Token<TToken>> Tokenize(string value)
        {
            var context = new StateContext<TToken>
            {
                Value = value,
                TokenType = default,
                Position = 0,
            };

            //var token = new Token<TToken>(new Token(string.Empty, 0, 0), default);

            while (Any())
            {
                foreach (var next in _stateTransition.Next(context.TokenType))
                {
                    context.TokenType = next.Token;
                    if (next.Match(context) is {} token)
                    {
                        yield return token;
                        // context = new StateContext<TToken>
                        // {
                        //     Value = value,
                        //     //TokenType = next.TokenType,
                        //     Position = context.Position + match.Length,
                        // };
                    }
                }
            }

            bool Any() => context.Position < value.Length - 1;
        }
    }


    public class StateTransition<TToken> where TToken : Enum
    {
        private readonly IImmutableDictionary<TToken, IImmutableList<State<TToken>>> _transitions;

        public StateTransition(IImmutableDictionary<TToken, IImmutableList<State<TToken>>> transitions) => _transitions = transitions;

        public IEnumerable<State<TToken>> Next(TToken token) => _transitions[token];
    }

    public class StateTransitionBuilder<TToken> : List<State<TToken>> where TToken : Enum
    {
        public void Add(TToken token, params TToken[] next) => Add(new State<TToken>(token, next));

        public StateTransition<TToken> Build()
        {
            var transitions = this.Aggregate(ImmutableDictionary<TToken, IImmutableList<State<TToken>>>.Empty, (mappings, state) =>
            {
                var nextStates =
                    from n in state.Next
                    join s in this on n equals s.Token
                    select s;

                return mappings.Add(state.Token, nextStates.ToImmutableList());
            });

            return new StateTransition<TToken>(transitions);
        }

        public static implicit operator StateTransition<TToken>(StateTransitionBuilder<TToken> builder) => builder.Build();
    }

    public class Token<TToken> where TToken : Enum
    {
        public Token(string value, int position, int length, TToken type)
        {
            (Value, Position, Length, Type) = (value, position, length, type);
        }

        public string Value { get; }

        public int Position { get; }

        public int Length { get; }

        public TToken Type { get; }

        //[DebuggerStepThrough]
        //public override string ToString() => $"{base.ToString()} {Type.ToString()}";
    }

    public interface ITokenMatcher
    {
        Token<TToken>? Match<TToken>(StateContext<TToken> context) where TToken : Enum;
    }

    public abstract class TokenMatcherAttribute : Attribute, ITokenMatcher
    {
        public abstract Token<TToken>? Match<TToken>(StateContext<TToken> context) where TToken : Enum;
    }

    // Can recognize regexable patterns.
    // The pattern requires one group that is the token to return. 
    public class RegexAttribute : TokenMatcherAttribute
    {
        private readonly Regex _regex;

        [DebuggerStepThrough]
        public RegexAttribute([RegexPattern] string prefixPattern) => _regex = new Regex($@"\G{prefixPattern}");

        public override Token<TToken>? Match<TToken>(StateContext<TToken> context)
        {
            if (_regex.Match(context.Value, context.Position) is var match && match.Success)
            {
                try
                {
                    return new Token<TToken>(match.Groups[1].Value, context.Position, match.Length, context.TokenType);
                }
                finally
                {
                    context.Position += match.Length;
                }
            }

            return default;
        }
    }

    // Can recognize constant patterns.
    public class ConstAttribute : TokenMatcherAttribute
    {
        private readonly string _pattern;

        public ConstAttribute(string pattern) => _pattern = pattern;

        public override Token<TToken>? Match<TToken>(StateContext<TToken> context)
        {
            return
                // All characters have to be matched.
                MatchLength() == _pattern.Length
                    ? new Token<TToken>(_pattern, context.Position, _pattern.Length, context.TokenType)
                    : default;

            int MatchLength() => _pattern.TakeWhile((t, i) => context.Value[context.Position + i].Equals(t)).Count();
        }
    }

    // Assists regex in tokenizing quoted strings because regex has no memory of what it has seen.
    // Requires two patterns:
    // - one for the separator because it has to know where the value begins
    // - the other for an unquoted value if it's not already quoted
    public class TextAttribute : TokenMatcherAttribute
    {
        public static readonly IImmutableSet<char> Escapables = new[] { '\\', '"', '\'' }.ToImmutableHashSet();

        [DebuggerStepThrough]
        public override Token<TToken>? Match<TToken>(StateContext<TToken> context)
        {
            var position = context.Position;
            var length = 0;

            // Skip whitespace.
            for (; context.Position < context.Value.Length && context.Value[context.Position] == ' '; context.Position++, length++) ;

            var cache = new StringBuilder();
            var escapeSequence = false;
            var quote = char.MinValue; // Opening/closing quote.

            for (; context.Position < context.Value.Length; context.Position++, length++)
            {
                var c = context.Value[context.Position];

                if (quote == char.MinValue)
                {
                    if (@"'""".Contains(c))
                    {
                        quote = c;
                    }
                    else
                    {
                        // It doesn't start with a quote.
                        return default;
                    }
                }
                else
                {
                    if (c == '\\' && !escapeSequence)
                    {
                        escapeSequence = true;
                    }
                    else
                    {
                        if (escapeSequence)
                        {
                            if (Escapables.Contains(c))
                            {
                                // Remove escape char. We don't need them in the result.
                                cache.Length--;
                            }

                            escapeSequence = false;
                        }
                        else
                        {
                            if (c == quote)
                            {
                                try
                                {
                                    return new Token<TToken>(cache.ToString(), position, length + 1, context.TokenType); // + the last quote
                                }
                                finally
                                {
                                    context.Position += 1;
                                }
                            }
                        }
                    }

                    cache.Append(c);
                }
            }

            return default;
        }
    }

    public abstract class TokenMatcherProviderAttribute : Attribute
    {
        public abstract IEnumerable<ITokenMatcher> GetMatchers<TToken>(TToken tokenType);
    }

    public class EnumTokenMatcherProviderAttribute : TokenMatcherProviderAttribute
    {
        public override IEnumerable<ITokenMatcher> GetMatchers<TToken>(TToken tokenType)
        {
            if (!typeof(TToken).IsEnum) throw new ArgumentException($"Token must by of Enum type.");

            return
                typeof(TToken)
                    .GetField(tokenType.ToString())
                    .GetCustomAttributes<TokenMatcherAttribute>();
        }
    }

    public class State<TToken> where TToken : Enum
    {
        private readonly IEnumerable<ITokenMatcher> _matchers;

        public State(TToken token, params TToken[] next)
        {
            Token = token;
            Next = next;
            _matchers =
                typeof(TToken)
                    .GetCustomAttribute<TokenMatcherProviderAttribute>()
                    .GetMatchers(token)
                    .ToList();
        }

        public TToken Token { get; }

        public IEnumerable<TToken> Next { get; }

        public Token<TToken>? Match(StateContext<TToken> context)
        {
            foreach (var matcher in _matchers)
            {
                if (matcher.Match(context) is {} token)
                {
                    return token;
                }
            }

            return default;
        }

        public override string ToString() => $"State: '{Token}', Next: [{string.Join(", ", Next)}]";
    }

    public class StateContext<TToken>
    {
        public string Value { get; set; }

        public TToken TokenType { get; set; }

        public int Position { get; set; }

        public string Read => Value.Substring(0, Position);

        public string Left => Value.Substring(Position, Value.Length - Position);

        //public override string ToString() => $"{nameof(Position)}={Position}, {nameof(TokenType)}={TokenType}";
    }
}