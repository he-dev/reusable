using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Xunit;

namespace Reusable.Experimental.TokenizerV6
{
    public interface ITokenizer<TToken> where TToken : Enum
    {
        IEnumerable<Token<TToken>> Tokenize(string value);
    }

    public class Tokenizer<TToken> : ITokenizer<TToken> where TToken : Enum
    {
        private readonly IImmutableDictionary<TToken, IImmutableList<State<TToken>>> _transitions;

        public Tokenizer(IImmutableList<State<TToken>> states)
        {
            _transitions = StateTransitionMapper.CreateTransitionMap(states);
        }

        public IEnumerable<Token<TToken>> Tokenize(string value)
        {
            var context = new StateContext<TToken>
            {
                Value = value,
                TokenType = default,
                Offset = 0,
            };


            while (Any())
            {
                var current = _transitions[context.TokenType];
                foreach (var next in current)
                {
                    if (next.Match(context) is {} match)
                    {
                        yield return new Token<TToken>(match, next.TokenType);
                        context = new StateContext<TToken>
                        {
                            Value = value,
                            TokenType = next.TokenType,
                            Offset = context.Offset + match.Length,
                        };
                    }
                }
            }

            bool Any() => context.Offset < value.Length - 1;
        }
    }

    public static class StateTransitionMapper
    {
        // Turns the adjacency-list of states into a dictionary for faster lookup.
        public static IImmutableDictionary<TToken, IImmutableList<State<TToken>>> CreateTransitionMap<TToken>(IImmutableList<State<TToken>> states) where TToken : Enum
        {
            return states.Aggregate(ImmutableDictionary<TToken, IImmutableList<State<TToken>>>.Empty, (mappings, state) =>
            {
                var nextStates =
                    from n in state.Next
                    join s in states on n equals s.TokenType
                    select s;

                return mappings.Add(state.TokenType, nextStates.ToImmutableList());
            });
        }
    }

    public class TokenMatch
    {
        public TokenMatch(string value, int offset, int length) => (Value, Offset, Length) = (value, offset, length);
        public string Value { get; }
        public int Offset { get; }
        public int Length { get; }
        public override string ToString() => $"'{Value}' at {Offset}, Length: {Length}";
    }

    public interface IMatcher
    {
        TokenMatch? Match(string value, int offset);
    }

    public abstract class MatcherAttribute : Attribute, IMatcher
    {
        public abstract TokenMatch? Match(string value, int offset);
    }

    // Can recognize regexable patterns.
    // The pattern requires one group that is the token to return. 
    public class RegexAttribute : MatcherAttribute
    {
        private readonly Regex _regex;

        public RegexAttribute([RegexPattern] string prefixPattern) => _regex = new Regex($@"\G{prefixPattern}");

        public override TokenMatch? Match(string value, int offset)
        {
            return
                _regex.Match(value, offset) is var match && match.Success
                    ? new TokenMatch(match.Groups[1].Value, offset, match.Length)
                    : default;
        }
    }

    // Can recognize constant patterns.
    public class ConstAttribute : MatcherAttribute
    {
        private readonly string _pattern;

        public ConstAttribute(string pattern) => _pattern = pattern;

        public override TokenMatch? Match(string value, int offset)
        {
            return
                // All characters have to be matched.
                MatchLength() == _pattern.Length
                    ? new TokenMatch(_pattern, offset, _pattern.Length)
                    : default;

            int MatchLength() => _pattern.TakeWhile((t, i) => value[offset + i].Equals(t)).Count();
        }
    }

    // Assists regex in tokenizing quoted strings because regex has no memory of what it has seen.
    // Requires two patterns:
    // - one for the separator because it has to know where the value begins
    // - the other for an unquoted value if it's not already quoted
    public class TextAttribute : MatcherAttribute
    {
        public static readonly IImmutableSet<char> Escapables = new[] { '\\', '"', '\'' }.ToImmutableHashSet();

        //private readonly Regex _prefixRegex;
        private readonly Regex _unquotedValuePattern;

//        public QTextAttribute([RegexPattern] string separatorPattern, [RegexPattern] string unquotedValuePattern = default)
//        {
//            _prefixRegex = new Regex($@"\G{separatorPattern}");
//            _unquotedValuePattern = new Regex($@"\G{unquotedValuePattern}");
//        }

        // public QTextAttribute([RegexPattern] string unquotedValuePattern = default)
        // {
        //     //_prefixRegex = new Regex($@"\G{separatorPattern}");
        //     _unquotedValuePattern = new Regex($@"\G{unquotedValuePattern}");
        // }

        public override TokenMatch Match(string value, int offset)
        {
            // if (_unquotedValuePattern.Match(value, offset) is var valueMatch && valueMatch.Groups[1].Success)
            // {
            //     return new MatchResult(valueMatch.Groups[1].Value, valueMatch.Length);
            // }
            // else
            return MatchQuoted(value, offset) is {} token ? token : default;
        }

        // "foo \"bar\" baz"
        // ^ start         ^ end
        private static TokenMatch MatchQuoted(string value, int offset)
        {
            var length = 0;

            // Skip whitespace.
            for (; offset < value.Length && value[offset] == ' '; offset++, length++) ;

            var token = new StringBuilder();
            var escapeSequence = false;
            var quote = char.MinValue; // Opening/closing quote.

            for (; offset < value.Length; offset++, length++)
            {
                var c = value[offset];

                if (quote == char.MinValue)
                {
                    if (@"'""".Contains(c))
                    {
                        quote = c;
                    }
                    else
                    {
                        // It doesn't start with a quote. This is unacceptable. Either an empty value or an unquoted one.
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
                                token.Length--;
                            }

                            escapeSequence = false;
                        }
                        else
                        {
                            if (c == quote)
                            {
                                // +2 because there were two quotes.
                                return new TokenMatch(token.ToString(), offset, length + 1); // + the last quote
                            }
                        }
                    }

                    token.Append(c);
                }
            }

            return default;
        }
    }
    
    public abstract class TokenMatcherProviderAttribute : Attribute
    {
        public abstract IEnumerable<IMatcher> GetMatchers<TToken>(TToken token);
    }

    public class EnumTokenMatcherProviderAttribute : TokenMatcherProviderAttribute
    {
        public override IEnumerable<IMatcher> GetMatchers<TToken>(TToken token)
        {
            if (!typeof(TToken).IsEnum) throw new ArgumentException($"Token must by of Enum type.");

            return
                typeof(TToken)
                    .GetField(token.ToString())
                    .GetCustomAttributes<MatcherAttribute>();
        }
    }

    public class State<TToken>
    {
        private readonly IEnumerable<IMatcher> _matchers;

        public State(TToken tokenType, params TToken[] next)
        {
            TokenType = tokenType;
            Next = next;
            _matchers =
                typeof(TToken)
                    .GetCustomAttribute<TokenMatcherProviderAttribute>()
                    .GetMatchers(tokenType)
                    .ToList();
        }

        public TToken TokenType { get; }

        public IEnumerable<TToken> Next { get; }

        public TokenMatch? Match(StateContext<TToken> stateContext)
        {
            foreach (var matcher in _matchers)
            {
                if (matcher.Match(stateContext.Value, stateContext.Offset) is {} token)
                {
                    return token;
                }
            }

            return default;
        }

        public override string ToString() => $"State: '{TokenType}', Next: [{string.Join(", ", Next)}]";
    }

    public class StateContext<TToken>
    {
        public string Value { get; set; }

        public TToken TokenType { get; set; }

        public int Offset { get; set; }

        public override string ToString() => $"{nameof(Offset)}={Offset}, {nameof(TokenType)}={TokenType}";
    }

    public class Token<TToken>
    {
        public Token(TokenMatch match, TToken type)
        {
            Index = match.Offset;
            Value = match.Value;
            Length = match.Length;
            Type = type;
        }

        public int Index { get; }

        public string Value { get; }

        public int Length { get; }

        public TToken Type { get; }

        public override string ToString() => $"{Type.ToString()} '{Value}' at {Index}, Length: {Length}";
    }
}

namespace Reusable.Experimental.TokenizerV6.CommandLine
{
    using static CommandLineToken;

    public class CommandLineTokenizerTest
    {
        private static readonly ITokenizer<CommandLineToken> Tokenizer = new CommandLineTokenizer();

        [Theory]
        [InlineData("cmd", "cmd")]
        [InlineData("cmd --arg", "cmd", "arg")]
        [InlineData("cmd --arg --arg", "cmd", "arg", "arg")]
        [InlineData("cmd --arg  val", "cmd", "arg", "val")]
        [InlineData("cmd --arg --arg val val", "cmd", "arg", "arg", "val", "val")]
        [InlineData("cmd --arg val val", "cmd", "arg", "val", "val")]
        [InlineData("cmd val val --arg  --arg val    val", "cmd", "val", "val", "arg", "arg", "val", "val")]
        [InlineData("cmd -f -f --arg  -fl", "cmd", "f", "f", "arg", "fl")]
        [InlineData("cmd --arg \"arg:val\"    \"val-arg\" -f --arg", "cmd", "arg", "arg:val", "val-arg", "f", "arg")]
        public void Can_tokenize_command_lines(string value, params string[] expected)
        {
            var tokens = Tokenizer.Tokenize(value).ToList();
            Assert.Equal(expected, tokens.Select(t => t.Value).ToArray());
        }
    }

    [EnumTokenMatcherProvider]
    public enum CommandLineToken
    {
        Start = 0,

        [Regex(@"(?:\A|\s+)([a-z][a-z0-9\+\.\-]+)"), Text]
        Value,

        [Regex(@"\s+--([a-z][a-z0-9\+\.\-]+)")]
        Argument,

        [Regex(@"\s+\-([a-z]+)")]
        Flag,
    }

    public class CommandLineTokenizer : Tokenizer<CommandLineToken>
    {
        /*

         command [-argument][=value][,value]
         
         command --------------------------- CommandLine
                \                           /
                 -argument ------   ------ /    
                          \      / \      /
                           =value   ,value
                           
          input ------ x ------------- x ----- x ---> command-line
                \     / \             / \     /
                 value   --arg ----- /   -flag
                              \     /
                               value                              
                    
        */
        private static readonly State<CommandLineToken>[] States =
        {
            new State<CommandLineToken>(default, Value),
            new State<CommandLineToken>(Value, Value, Argument, Flag),
            new State<CommandLineToken>(Argument, Argument, Value, Flag),
            new State<CommandLineToken>(Flag, Flag, Argument),
        };

        public CommandLineTokenizer() : base(States.ToImmutableList()) { }
    }
}