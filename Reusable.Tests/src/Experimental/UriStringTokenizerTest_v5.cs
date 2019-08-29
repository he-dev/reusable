using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Xunit;

namespace Reusable.Experimental.TokenizerV5
{
    public interface ITokenizer<TToken> where TToken : Enum
    {
        Node<Token<TToken>> Tokenize(string value);
    }

    public class Tokenizer<TToken> : ITokenizer<TToken> where TToken : Enum
    {
        private readonly IImmutableDictionary<TToken, IImmutableList<State<TToken>>> _transitions;

        public Tokenizer(IImmutableList<State<TToken>> states)
        {
            _transitions = StateTransitionMapper.CreateTransitionMap(states);
        }

        public Node<Token<TToken>> Tokenize(string value)
        {
            var state = _transitions[default].Single();
            var offset = 0;

            var root = new Node<Token<TToken>>(new Token<TToken>(string.Empty, default, default, default));
            state.Match(new TokenizerContext<TToken>
            {
                Value = value,
                Next = tokenType => _transitions[tokenType]
            }, new TokenContext<TToken>
            {
                Node = root,
                Offset = 0
            });

            return root;
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
                    join s in states on n equals s.Token
                    select s;

                return mappings.Add(state.Token, nextStates.ToImmutableList());
            });
        }
    }

    public class MatchResult<TToken>
    {
        public MatchResult(string token, int length, TToken tokenType)
        {
            Success = true;
            Token = token;
            Length = length;
            TokenType = tokenType;
        }

        public static MatchResult<TToken> Failure(TToken tokenType) => new MatchResult<TToken>(string.Empty, 0, tokenType) { Success = false };

        public bool Success { get; private set; }

        public string Token { get; }

        public int Length { get; }

        public TToken TokenType { get; }

        public override string ToString() => $"{TokenType}: {Success} -> '{Token}', Length: {Length}";

        public static implicit operator bool(MatchResult<TToken> result) => result.Success;
    }

    public interface IMatcher
    {
        MatchResult<TToken> Match<TToken>(string value, int offset, TToken tokenType);
    }

    public abstract class MatcherAttribute : Attribute, IMatcher
    {
        public abstract MatchResult<TToken> Match<TToken>(string value, int offset, TToken tokenType);
    }

    // Can recognize regexable patterns.
    // The pattern requires one group that is the token to return. 
    public class RegexAttribute : MatcherAttribute
    {
        private readonly Regex _regex;

        public RegexAttribute([RegexPattern] string prefixPattern)
        {
            _regex = new Regex($@"\G{prefixPattern}");
        }

        public override MatchResult<TToken> Match<TToken>(string value, int offset, TToken tokenType)
        {
            return
                _regex.Match(value, offset) is var match && match.Success
                    ? new MatchResult<TToken>(match.Groups[1].Value, match.Length, tokenType)
                    : MatchResult<TToken>.Failure(tokenType);
        }
    }

    // Can recognize constant patterns.
    public class ConstAttribute : MatcherAttribute
    {
        private readonly string _pattern;

        public ConstAttribute(string pattern) => _pattern = pattern;

        public override MatchResult<TToken> Match<TToken>(string value, int offset, TToken tokenType)
        {
            return
                // All characters have to be matched.
                MatchLength() == _pattern.Length
                    ? new MatchResult<TToken>(_pattern, _pattern.Length, tokenType)
                    : MatchResult<TToken>.Failure(tokenType);

            int MatchLength() => _pattern.TakeWhile((t, i) => value[offset + i].Equals(t)).Count();
        }
    }

    // Assists regex in tokenizing quoted strings because regex has no memory of what it has seen.
    // Requires two patterns:
    // - one for the separator because it has to know where the value begins
    // - the other for an unquoted value if it's not already quoted
    public class QTextAttribute : MatcherAttribute
    {
        public static readonly IImmutableSet<char> Escapables = new[] { '\\', '"', '\'' }.ToImmutableHashSet();

        //private readonly Regex _prefixRegex;
        private readonly Regex _unquotedValuePattern;

//        public QTextAttribute([RegexPattern] string separatorPattern, [RegexPattern] string unquotedValuePattern = default)
//        {
//            _prefixRegex = new Regex($@"\G{separatorPattern}");
//            _unquotedValuePattern = new Regex($@"\G{unquotedValuePattern}");
//        }

        public QTextAttribute([RegexPattern] string unquotedValuePattern = default)
        {
            //_prefixRegex = new Regex($@"\G{separatorPattern}");
            _unquotedValuePattern = new Regex($@"\G{unquotedValuePattern}");
        }

        public override MatchResult<TToken> Match<TToken>(string value, int offset, TToken tokenType)
        {
            if (_unquotedValuePattern.Match(value, offset) is var valueMatch && valueMatch.Groups[1].Success)
            {
                return new MatchResult<TToken>(valueMatch.Groups[1].Value, valueMatch.Length, tokenType);
            }
            else
            {
                if (MatchQuoted(value, offset, tokenType) is var matchQuoted && matchQuoted.Success)
                {
                    return matchQuoted;
                }
            }

            return MatchResult<TToken>.Failure(tokenType);
        }

        // "foo \"bar\" baz"
        // ^ start         ^ end
        private static MatchResult<TToken> MatchQuoted<TToken>(string value, int offset, TToken tokenType)
        {
            var token = new StringBuilder();
            var escapeSequence = false;
            var quote = '\0'; // Opening/closing quote.

            foreach (var (c, i) in value.SkipFastOrDefault(offset).SelectIndexed())
            {
                if (i == 0)
                {
                    if (@"'""".Contains(c))
                    {
                        quote = c;
                    }
                    else
                    {
                        // It doesn't start with a quote. This is unacceptable. Either an empty value or an unquoted one.
                        return MatchResult<TToken>.Failure(tokenType);
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
                                return new MatchResult<TToken>(token.ToString(), i + 2, tokenType);
                            }
                        }
                    }

                    token.Append(c);
                }
            }

            return MatchResult<TToken>.Failure(tokenType);
        }
    }

    public static class StringExtensions
    {
        // Doesn't enumerate the string from the beginning for skipping.
        public static IEnumerable<char> SkipFastOrDefault(this string source, int offset)
        {
            // Who uses for-loop these days? Let's hide it here so nobody can see this monster.
            for (var i = offset; i < source.Length; i++)
            {
                yield return source[i];
            }
        }

        // Doesn't enumerate a collection from the beginning if it implements `IList<T>`.
        // Falls back to the default `Skip`.
        public static IEnumerable<T> SkipFastOrDefault<T>(this IEnumerable<T> source, int offset)
        {
            // Even more for-loops to hide.
            switch (source)
            {
                case IList<T> list:
                    for (var i = offset; i < list.Count; i++)
                    {
                        yield return list[i];
                    }

                    break;

                default:
                    foreach (var item in source.Skip(offset))
                    {
                        yield return item;
                    }

                    break;
            }
        }
    }

    public static class EnumerableExtensions
    {
        // This is so common that it deserves its own extension.
        public static IEnumerable<(T Item, int Index)> SelectIndexed<T>(this IEnumerable<T> source)
        {
            return source.Select((c, i) => (c, i));
        }
    }

    public class ContextAttribute : Attribute
    {
        public ContextAttribute(object context) => Value = context;

        public object Value { get; }
    }

    public abstract class TokenInfoAttribute : Attribute
    {
        public abstract IMatcher GetMatcher<TToken>(TToken token);
    }

    public class EnumTokenInfoAttribute : TokenInfoAttribute
    {
        public override IMatcher GetMatcher<TToken>(TToken token)
        {
            if (!typeof(TToken).IsEnum) throw new ArgumentException($"Token must by of Enum type.");

            return
                typeof(TToken)
                    .GetField(token.ToString())
                    .GetCustomAttribute<MatcherAttribute>();
        }
    }

    public class State<TToken>
    {
        private readonly IMatcher _matcher;

        public State(TToken token, params TToken[] next)
        {
            Token = token;
            Next = next;
            _matcher =
                typeof(TToken)
                    .GetCustomAttribute<TokenInfoAttribute>()
                    .GetMatcher(token);
        }

        public TToken Token { get; }

        public IEnumerable<TToken> Next { get; }

        public object Context { get; set; }

        public bool Match(TokenizerContext<TToken> tokenizerContext, TokenContext<TToken> tokenContext)
        {
            if (tokenizerContext.Current?.Equals(tokenContext.Value) == false)
            {
                //return false;
            }

            if (_matcher.Match(tokenizerContext.Value, tokenContext.Offset, Token) is var match && !match)
            {
                return false;
            }

            var token = new Token<TToken>(match.Token, match.Length, tokenContext.Offset, match.TokenType);
            var node = new Node<Token<TToken>>(token);
            tokenContext.Node.Add(node);

            if (tokenContext.Offset + match.Length is var offset && offset > tokenizerContext.Value.Length - 1)
            {
                return false;
            }

            foreach (var state in tokenizerContext.Next(match.TokenType))
            {
                tokenContext = new TokenContext<TToken>
                {
                    Value = state.Context ?? tokenizerContext.Current,
                    Node = state.Context is null ? tokenContext.Node : node,
                    Offset = offset,
                };
                using (tokenizerContext.Push(Context))
                {
                    if (state.Match(tokenizerContext, tokenContext))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override string ToString() => $"State: '{Token}', Next: [{string.Join(", ", Next)}]";
    }

    public class TokenizerContext<TToken> : IEnumerable<object>
    {
        private readonly Stack<object> _contexts;

        public TokenizerContext()
        {
            _contexts = new Stack<object>();
        }

        public string Value { get; set; }

        public Func<TToken, IEnumerable<State<TToken>>> Next { get; set; }

        public object Current => _contexts.Any() ? _contexts.Peek() : null;

        public IDisposable Push(object context)
        {
            if (context is null) return Disposable.Empty;

            _contexts.Push(context);
            return Disposable.Create(() => _contexts.Pop());
        }

        public override string ToString() => $"Value: '{Value}', Context: '{Current}'";

        public IEnumerator<object> GetEnumerator() => _contexts.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_contexts).GetEnumerator();
    }

    public class TokenContext<TToken>
    {
        public object Value { get; set; }
        public Node<Token<TToken>> Node { get; set; }
        public int Offset { get; set; }

        public override string ToString() => $"{nameof(Offset)}={Offset}, Context: '{Value}'";
    }

    public class Token<TToken>
    {
        public Token(string token, int length, int index, TToken type)
        {
            Text = token;
            Length = length;
            Index = index;
            Type = type;
        }

        public int Index { get; }

        public int Length { get; }

        public string Text { get; }

        public TToken Type { get; }

        public override string ToString() => $"{Type.ToString()} '{Text}' at {Index}, Length: {Length}";
    }
}

namespace Reusable.Experimental.TokenizerV5.UriString
{
    using static UriToken;

    public class UriStringParserTest
    {
        private static readonly ITokenizer<UriToken> Tokenizer = new UriStringTokenizer();

        [Theory]
        [InlineData(
            "scheme://user@host:123/pa/th?key-1=val-1&key-2=val-2#f",
            "scheme //user host 123/pa/th key-1 val-1 key-2 val-2 f")]
        [InlineData(
            "scheme://user@host:123/pa/th?key-1=val-1&key-2=val-2",
            "scheme //user host 123/pa/th key-1 val-1 key-2 val-2")]
        [InlineData(
            "scheme://user@host:123/pa/th?key-1=val-1",
            "scheme //user host 123/pa/th key-1 val-1")]
        [InlineData(
            "scheme://user@host:123/pa/th",
            "scheme //user host 123/pa/th")]
        [InlineData(
            "scheme:///pa/th",
            "scheme ///pa/th"
        )]
        public void Can_tokenize_URIs(string uri, string expected)
        {
            //var tokens = Tokenizer.Tokenize(uri).ToList();
            //var actual = string.Join("", tokens.Select(t => t.Text));
            //Assert.Equal(expected.Replace(" ", string.Empty), actual);
        }

        [Fact]
        public void Throws_when_invalid_character()
        {
// Using single letters for faster debugging.
            var uri = "s://:u@h:1/p?k=v&k=v#f";
//             ^ - invalid character
            var ex = Assert.Throws<ArgumentException>(() => Tokenizer.Tokenize(uri).ToList());
            Assert.Equal("Invalid character ':' at 4.", ex.Message);
        }
    }

    public class UriStringTokenizer : Tokenizer<UriToken>
    {
/*
 
 scheme:[//[userinfo@]host[:port]]path[?key=value&key=value][#fragment]
        [ ----- authority ----- ]     [ ----- query ------ ]
  
 scheme: ------------------------ '/'path -------------------------  --------- UriString
        \                         /      \                         /\         /
         // --------- host ----- /        ?key ------ &key ------ /  #fragment
           \         /    \     /             \      /    \      /
            userinfo@      :port               =value      =value             
  
*/
        private static readonly State<UriToken>[] States =
        {
            new State<UriToken>(default, Scheme),
            new State<UriToken>(Scheme, AuthorityPrefix, Path),
            new State<UriToken>(AuthorityPrefix, UserInfo, Host, Path),
            new State<UriToken>(UserInfo, Host),
            new State<UriToken>(Host, Port, Path),
            new State<UriToken>(Port, Path),
            new State<UriToken>(Path, Key, Fragment),
            new State<UriToken>(Key, Value, Fragment),
            new State<UriToken>(Value, Key, Fragment),
            new State<UriToken>(Fragment, Fragment),
        };

        public UriStringTokenizer() : base(States.ToImmutableList()) { }
    }

    [EnumTokenInfo]
    public enum UriToken
    {
        Start = 0,

        [Regex(@"([a-z0-9\+\.\-]+):")]
        Scheme,

        [Const("//")]
        AuthorityPrefix,

        [Regex(@"([a-z0-9_][a-z0-9\.\-_:]+)@")]
        UserInfo,

        [Regex(@"([a-z0-9\.\-_]+)")]
        Host,

        [Regex(@":([0-9]*)")]
        Port,

        [Regex(@"(\/?[a-z_][a-z0-9\/:\.\-\%_@]+)")]
        Path,

        [Regex(@"[\?\&\;]([a-z0-9\-]*)")]
        Key,

        [Regex(@"=([a-z0-9\-]*)")]
        Value,

        [Regex(@"#([a-z]*)")]
        Fragment,
    }
}

namespace Reusable.Experimental.TokenizerV5.CommandLine
{
    using static CommandLineToken;

    public class CommandLineTokenizerTest
    {
        private static readonly ITokenizer<CommandLineToken> Tokenizer = new CommandLineTokenizer();

        [Theory]
        [InlineData(
            "cmd",
            "cmd")]
        [InlineData(
            "cmd -arg",
            "cmd -arg")]
        [InlineData(
            "cmd -arg -arg",
            "cmd -arg -arg")]
        [InlineData(
            "cmd -a1=v1",
            "cmd -a1=v1")]
        [InlineData(
            "cmd -a1 -a2=v1,v2",
            "cmd -a1 -a2=v1,v2")]
        [InlineData(
            "command -argument value value",
            "command  argument value value")]
        [InlineData(
            "command -argument:value,value",
            "command  argument value value")]
        [InlineData(
            "command -argument=value",
            "command  argument value")]
        [InlineData(
            "command -argument:value,value",
            "command  argument value value")]
        [InlineData(
            @"command -argument=""foo--bar"",value -argument value",
            @"command  argument   foo--bar   value  argument value")]
        [InlineData(
            @"command -argument=""foo--\""bar"",value -argument value",
            @"command  argument   foo-- ""bar   value  argument value")]
        public void Can_tokenize_command_lines(string value, string expected)
        {
            var tokens = Tokenizer.Tokenize(value);
            var views = tokens.Views((t, depth) => new NodePlainView { Text = t?.Text ?? "blub", Depth = depth });
            var rendered = views.Render();
            var text = string.Join("", views.Select(v => v.Text)).Trim();
            //var actual = string.Join("", tokens.Select(t => t.Text));
            Assert.Equal(value, text);
        }

//        [Fact]
//        public void Throws_when_invalid_character()
//        {
//            // Using single letters for faster debugging.
//            var uri = "s://:u@h:1/p?k=v&k=v#f";
//            //             ^ - invalid character
//
//            var ex = Assert.Throws<ArgumentException>(() => Tokenizer.Tokenize(uri).ToList());
//            Assert.Equal("Invalid character ':' at 4.", ex.Message);
//        }
    }

    [EnumTokenInfo]
    public enum CommandLineToken
    {
        Start = 0,

        [Regex(@"\s*(\?|[a-z0-9][a-z0-9\-_]*)")]
        Command,

        //[Context("Argument")]
        [Regex(@"(\s*[\-\.\/])")]
        ArgumentSeparator,

        [Context("Argument")]
        [Regex(@"([a-z0-9][a-z0-9\-_]*)")]
        Argument,

        //[Context("Value")]
        [Regex(@"([\=\:]|\,?\s*)")]
        ValueSeparator,

        [Context("Value")]
        [QText(@"([a-z0-9][a-z0-9\-_]*)")]
        Value,
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
                    
        */
        private static readonly State<CommandLineToken>[] States =
        {
            new State<CommandLineToken>(default, Command) { Context = "Command" },
            new State<CommandLineToken>(Command, ArgumentSeparator) { },
            new State<CommandLineToken>(ArgumentSeparator, Argument) { Context = "Argument" },
            new State<CommandLineToken>(Argument, ArgumentSeparator, ValueSeparator),
            new State<CommandLineToken>(ValueSeparator, Value) { Context = "Value" },
            new State<CommandLineToken>(Value, ArgumentSeparator, ValueSeparator),
        };

        public CommandLineTokenizer() : base(States.ToImmutableList()) { }
    }
}