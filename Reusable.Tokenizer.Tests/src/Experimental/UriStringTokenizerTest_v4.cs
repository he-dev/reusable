using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Xunit;

namespace Reusable.Experimental.TokenizerV4
{
    public class HtmlStringTokenizerTest
    {
        [Fact]
        public void Can_tokenize_html()
        {
            //            // Using single letters for faster debugging.
            //            var html = "<p>before<!-- comment -->after</p>";
            //            var tokens = HtmlStringTokenizer.Tokenize(html).ToList();
            //
            //            Assert.Equal(11, tokens.Count);
            //            var actual = string.Join("", tokens.Select(t => t.Text));
            //
            //            Assert.Equal(html, actual);
        }
    }



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
            var state = _transitions[default];
            var offset = 0;

            while (Any())
            {
                // Using a switch because it looks good here. 
                switch (state.Select(s => s.Match(value, offset)).FirstOrDefault(m => m.Success))
                {
                    case null:
                        throw new ArgumentException($"Invalid character '{value[offset]}' at {offset}.");

                    case MatchResult<TToken> match:
                        yield return new Token<TToken>(match.Token, match.Length, offset, match.TokenType);
                        offset += match.Length;
                        state = _transitions[match.TokenType];
                        break;
                }
            }

            // Let's hide this ugly expression behind this nice helper.
            bool Any() => offset < value.Length - 1;
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

        private readonly Regex _prefixRegex;
        private readonly Regex _unquotedValuePattern;

        public QTextAttribute([RegexPattern] string separatorPattern, [RegexPattern] string unquotedValuePattern)
        {
            _prefixRegex = new Regex($@"\G{separatorPattern}");
            _unquotedValuePattern = new Regex($@"\G{unquotedValuePattern}");
        }

        public override MatchResult<TToken> Match<TToken>(string value, int offset, TToken tokenType)
        {
            if (_prefixRegex.Match(value, offset) is var prefixMatch && prefixMatch.Success)
            {
                if (MatchQuoted(value, offset + prefixMatch.Length, tokenType) is var matchQuoted && matchQuoted.Success)
                {
                    return matchQuoted;
                }
                else
                {
                    if (_unquotedValuePattern.Match(value, offset + prefixMatch.Length) is var valueMatch && valueMatch.Groups[1].Success)
                    {
                        return new MatchResult<TToken>(valueMatch.Groups[1].Value, prefixMatch.Length + valueMatch.Length, tokenType);
                    }
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

    public abstract class MatcherProviderAttribute : Attribute
    {
        public abstract IMatcher GetMatcher<TToken>(TToken token);
    }

    public class EnumMatcherProviderAttribute : MatcherProviderAttribute
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

    public class State<TToken> where TToken : Enum
    {
        private readonly IMatcher _matcher;

        public State(TToken token, params TToken[] next)
        {
            Token = token;
            Next = next;
            _matcher =
                typeof(TToken)
                    .GetCustomAttribute<MatcherProviderAttribute>()
                    .GetMatcher(token);
        }

        public TToken Token { get; }

        public IEnumerable<TToken> Next { get; }

        public MatchResult<TToken> Match(string value, int offset) => _matcher.Match(value, offset, Token);

        public override string ToString() => $"{Token} --> [{string.Join(", ", Next)}]";
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

        public override string ToString() => $"{Index}: {Text} ({Type})";
    }
}

namespace Reusable.Experimental.TokenizerV4.UriString
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
            var tokens = Tokenizer.Tokenize(uri).ToList();
            var actual = string.Join("", tokens.Select(t => t.Text));
            Assert.Equal(expected.Replace(" ", string.Empty), actual);
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

    [EnumMatcherProvider]
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

namespace Reusable.Experimental.TokenizerV4.CommandLine
{
    using static CommandLineToken;

    public class CommandLineTokenizerTest
    {
        private static readonly ITokenizer<CommandLineToken> Tokenizer = new CommandLineTokenizer();

        [Theory]
        [InlineData(
            "command -argument value -argument",
            "command  argument value argument")]
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
        public void Can_tokenize_command_lines(string uri, string expected)
        {
            var tokens = Tokenizer.Tokenize(uri).ToList();
            var actual = string.Join("", tokens.Select(t => t.Text));
            Assert.Equal(expected.Replace(" ", string.Empty), actual);
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

    [EnumMatcherProvider]
    public enum CommandLineToken
    {
        Start = 0,

        [Regex(@"\s*(\?|[a-z0-9][a-z0-9\-_]*)")]
        Command,

        [Regex(@"\s*[\-\.\/]([a-z0-9][a-z\-_]*)")]
        Argument,

        [QText(@"([\=\:\,]|\,?\s*)", @"([a-z0-9\.\;\-]+)")]
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
            new State<CommandLineToken>(default, Command),
            new State<CommandLineToken>(Command, Argument),
            new State<CommandLineToken>(Argument, Argument, Value),
            new State<CommandLineToken>(Value, Argument, Value),
        };

        public CommandLineTokenizer() : base(States.ToImmutableList()) { }
    }
}