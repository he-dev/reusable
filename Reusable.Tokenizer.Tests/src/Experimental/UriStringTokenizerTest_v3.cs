using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Xunit;

namespace Reusable.Experimental.TokenizerV3
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

    public static class StateTransitionMapper
    {
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
            var current = _transitions[default];

            for (var i = 0; i < value.Length;)
            {
                var matches =
                    from state in current
                    let token = state.Consume(value, i)
                    // Consider only non-empty tokens.
                    where token.Length > 0
                    select (state, token);

                if (matches.FirstOrDefault() is var match && match.token is null)
                {
                    throw new ArgumentException($"Invalid character '{value[i]}' at {i}.");
                }
                else
                {
                    if (match.state.IsToken)
                    {
                        yield return match.token;
                    }

                    i += match.token.Length;
                    current = _transitions[match.state.Token];
                }
            }
        }
    }

    public delegate (bool Success, string Token, int Length) MatchDelegate(string value, int offset);

    public abstract class MatcherAttribute : Attribute
    {
        public abstract (bool Success, string Token, int Length) Match(string value, int offset);
    }

    public class RegexAttribute : MatcherAttribute
    {
        private readonly Regex _regex;

        public RegexAttribute([RegexPattern] string pattern)
        {
            _regex = new Regex(pattern);
        }

        public override (bool Success, string Token, int Length) Match(string value, int offset)
        {
            var match = _regex.Match(value, offset);
            // Make sure the match was at the offset.
            return (match.Success && match.Index == offset, match.Groups[1].Value, match.Length);
        }
    }

    public class ConstAttribute : MatcherAttribute
    {
        private readonly string _pattern;

        public ConstAttribute(string pattern) => _pattern = pattern;

        public override (bool Success, string Token, int Length) Match(string value, int offset)
        {
            var matchCount = _pattern.TakeWhile((t, i) => value[offset + i].Equals(t)).Count();
            // All characters have to be matched.
            return (matchCount == _pattern.Length, _pattern, matchCount);
        }
    }

    // "foo \"bar\" baz"
    // ^ starts here   ^ ends here
    public class QTextAttribute : RegexAttribute
    {
        public static readonly IImmutableSet<char> Escapables = new[] { '\\', '"' }.ToImmutableHashSet();

        public QTextAttribute([RegexPattern] string pattern) : base(pattern) { }

        public override (bool Success, string Token, int Length) Match(string value, int offset)
        {
            return
                value[offset] == '"'
                    ? MatchQuoted(value, offset)
                    : base.Match(value, offset);
        }

        private (bool Success, string Token, int Length) MatchQuoted(string value, int offset)
        {
            var token = new StringBuilder();
            var escapeSequence = false;
            var quote = false;

            for (var i = offset; i < value.Length; i++)
            {
                var c = value[i];

                switch (c)
                {
                    case '"' when !escapeSequence:

                        switch (i == offset)
                        {
                            // Entering quoted text.
                            case true:
                                quote = !quote;
                                continue; // Don't eat quotes.

                            // End of quoted text.
                            case false:
                                return (true, token.ToString(), i - offset + 1);
                        }

                        break; // Makes the compiler happy.

                    case '\\' when !escapeSequence:
                        escapeSequence = true;
                        break;

                    default:

                        switch (escapeSequence)
                        {
                            case true:
                                switch (Escapables.Contains(c))
                                {
                                    case true:
                                        // Remove escape char.
                                        token.Length--;
                                        break;
                                }

                                escapeSequence = false;
                                break;
                        }

                        break;
                }

                token.Append(c);
            }

            return (false, token.ToString(), 0);
        }
    }

    public class State<TToken> where TToken : Enum
    {
        private readonly MatchDelegate _match;

        public State(TToken token, params TToken[] next)
        {
            Token = token;
            Next = next;
            _match =
                typeof(TToken)
                    .GetField(token.ToString())
                    .GetCustomAttribute<MatcherAttribute>() is MatcherAttribute matcher
                    ? (MatchDelegate)(matcher.Match)
                    : (MatchDelegate)((value, offset) => (false, string.Empty, 0));
        }

        public bool IsToken { get; set; } = true;

        public TToken Token { get; }

        public IEnumerable<TToken> Next { get; }

        public Token<TToken> Consume(string value, int offset)
        {
            return new Token<TToken>(_match(value, offset))
            {
                Type = Token,
                Index = offset
            };
        }

        public override string ToString() => $"{Token} --> [{string.Join(", ", Next)}]";
    }

    public class Token<TToken> where TToken : Enum
    {
        public Token((bool Success, string Token, int Length) match)
        {
            Length = match.Success ? match.Length : 0;
            Text = match.Success ? match.Token : string.Empty;
        }

        public int Index { get; set; }

        public int Length { get; set; }

        public string Text { get; set; }

        public TToken Type { get; set; }

        public override string ToString() => $"{Index}: {Text} ({Type})";
    }
}

namespace Reusable.Experimental.TokenizerV3.UriString
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

namespace Reusable.Experimental.TokenizerV3.CommandLine
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

    public enum CommandLineToken
    {
        Start = 0,

        [Regex(@"\s*(\?|[a-z0-9][a-z0-9\-_]*)")]
        Command,

        [Regex(@"\s*[\-\.\/]([a-z0-9][a-z\-_]*)")]
        Argument,

        [Regex(@"[\=\:\,\s]")]
        ValueBegin,

        [QText(@"([a-z0-9\.\;\-]*)")]
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
            new State<CommandLineToken>(Argument, Argument, ValueBegin),
            new State<CommandLineToken>(ValueBegin, Value) { IsToken = false },
            new State<CommandLineToken>(Value, Argument, ValueBegin),
        };

        public CommandLineTokenizer() : base(States.ToImmutableList()) { }
    }
}

namespace Reusable.Experimental.TokenizerV3.Html
{
    //    public enum HtmlToken
//    {
//        [Regex(@"(<(?:[a-z]+|!--|\/)?)")]
//        TagOpen,
//
//        [Regex(@"((?:\/|--)?>)")]
//        TagClose,
//
//        //[Regex(@"([a-z]+)")]
//        //TagName,
//
//        [Regex(@"([a-z\s]+)")]
//        Text,
//    }
//
//    public static class HtmlStringTokenizer
//    {
//        public static readonly ICollection<State<HtmlToken>> States = new[]
//        {
//            new State<HtmlToken>(TagOpen, TagClose),
//            //new State<HtmlToken>(TagName, TagClose),
//            new State<HtmlToken>(TagClose, Text, TagOpen),
//            new State<HtmlToken>(Text, TagOpen),
//        };
//
////        public static IEnumerable<Token<HtmlToken>> Tokenize(string value)
////        {
////            return Tokenizer.Tokenize(value, States);
////        }
//    }
}