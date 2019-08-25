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
    using static UriToken;
    using static HtmlToken;

    public class UriStringParserTest
    {
        [Fact]
        public void Can_tokenize_full_URI()
        {
            // Using single letters for faster debugging.
            var uri = "s://u@h:1/p?k=v&k=v#f";
            var tokens = UriStringTokenizer.Tokenize(uri).ToList();

            var expectedTokens = new[]
            {
                Scheme,
                AuthorityPrefix,
                UserInfo,
                Host,
                Port,
                Path,
                Key,
                Value,
                Key,
                Value,
                Fragment
            };

            Assert.Equal(expectedTokens, tokens.Select(t => t.Type).ToArray());

            //var actual = string.Join("", tokens.Select(t => t.Text));

            Assert.Equal("s//uh1/pkvkvf", string.Join("", tokens.Select(t => t.Text)));
        }

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
        public void Can_tokenize_partial_URI(string uri, string expected)
        {
            // Using single letters for faster debugging.
            var tokens = UriStringTokenizer.Tokenize(uri).ToList();
            var actual = string.Join("", tokens.Select(t => t.Text));
            Assert.Equal(expected.Replace(" ", string.Empty), actual);
        }

        [Fact]
        public void Can_tokenize_authority_and_path()
        {
            var uri = "s:///p";
            var tokens = UriStringTokenizer.Tokenize(uri).ToList();
            //Assert.Equal(4, tokens.Count);
            //var actual = string.Join("", tokens.Select(t => t.Text));
            //Assert.Equal(uri, actual);
            Assert.Equal(new[] { "s", "//", "/p" }, tokens.Select(t => t.Text).ToArray());
        }

        [Fact]
        public void Throws_when_invalid_character()
        {
            // Using single letters for faster debugging.
            var uri = "s://:u@h:1/p?k=v&k=v#f";
            //             ^ - invalid character

            var ex = Assert.Throws<ArgumentException>(() => UriStringTokenizer.Tokenize(uri).ToList());
            Assert.Equal("Invalid character ':' at 4.", ex.Message);
        }
    }

    public class HtmlStringTokenizerTest
    {
        [Fact]
        public void Can_tokenize_html()
        {
            // Using single letters for faster debugging.
            var html = "<p>before<!-- comment -->after</p>";
            var tokens = HtmlStringTokenizer.Tokenize(html).ToList();

            Assert.Equal(11, tokens.Count);
            var actual = string.Join("", tokens.Select(t => t.Text));

            Assert.Equal(html, actual);
        }
    }

    public static class Tokenizer
    {
        public static IEnumerable<Token<TToken>> Tokenize<TToken>(string value, IEnumerable<State<TToken>> states)
        {
            states = states.ToList(); // Materialize states.

            var current = states.Take(1).ToList();

            for (var i = 0; i < value.Length;)
            {
                var matches =
                    from state in current
                    let token = state.Consume(value, i)
                    // Consider only non-empty tokens.
                    where token.Length > 0
                    select (state, token);

                matches = matches.ToList(); // just for debugging

                if (matches.FirstOrDefault() is var match && match.token is null)
                {
                    throw new ArgumentException($"Invalid character '{value[i]}' at {i}.");
                }
                else
                {
                    yield return match.token;

                    // Advance by token's length.
                    i += match.token.Length;

                    // Get states that follow.
                    var nextStates =
                        from next in match.state.Next
                        join state in states on next equals state.Token
                        select state;

                    current = nextStates.ToList();
                }
            }
        }
    }

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

    public class State<TToken>
    {
        public State(TToken token, params TToken[] next)
        {
            Token = token;
            // Always add self-transition.
            Next = next.Prepend(token).ToList();
            Matcher = 
                typeof(TToken)
                    .GetField(token.ToString())
                    .GetCustomAttribute<MatcherAttribute>();
        }

        public TToken Token { get; set; }

        public IEnumerable<TToken> Next { get; set; }

        public MatcherAttribute Matcher { get; }

        public Token<TToken> Consume(string value, int offset)
        {
            var (success, text, length) = Matcher.Match(value, offset);

            return
                success
                    ? new Token<TToken>
                    {
                        Type = Token,
                        Index = offset,
                        Length = length,
                        Text = text
                    }
                    : new Token<TToken>
                    {
                        Type = Token,
                        Index = offset,
                        Length = 0,
                        Text = string.Empty
                    };
        }

        public override string ToString() => $"{Token} --> [{string.Join(", ", Next)}]";
    }

    public class Token<TToken>
    {
        public int Index { get; set; }

        public int Length { get; set; }

        public string Text { get; set; }

        public TToken Type { get; set; }

        public override string ToString() => $"{Index}: {Text} ({Type})";
    }

    public static class UriStringTokenizer
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

        public static readonly ICollection<State<UriToken>> States = new[]
        {
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

        public static IEnumerable<Token<UriToken>> Tokenize(string value)
        {
            return Tokenizer.Tokenize(value, States);
        }
    }

    public enum UriToken
    {
        [Regex(@"([a-z0-9\+\.\-]+):")]
        Scheme,

        [Const("//")]
        AuthorityPrefix,

        [Regex(@"([a-z0-9\.\-_:]+)@")]
        UserInfo,

        [Regex(@"([a-z0-9\.\-_]+)")]
        Host,

        [Regex(@":([0-9]*)")]
        Port,
        
        [Regex(@"(\/?[a-z0-9\/:\.\-\%_@]+)")]
        Path,

        [Regex(@"[\?\&\;]([a-z0-9\-]*)")]
        Key,

        [Regex(@"=([a-z0-9\-]*)")]
        Value,

        [Regex(@"#([a-z]*)")]
        Fragment,
    }

    public static class HtmlStringTokenizer
    {
        public static readonly ICollection<State<HtmlToken>> States = new[]
        {
            new State<HtmlToken>(TagOpen, TagClose),
            //new State<HtmlToken>(TagName, TagClose),
            new State<HtmlToken>(TagClose, Text, TagOpen),
            new State<HtmlToken>(Text, TagOpen),
        };

        public static IEnumerable<Token<HtmlToken>> Tokenize(string value)
        {
            return Tokenizer.Tokenize(value, States);
        }
    }

    public enum HtmlToken
    {
        [Regex(@"(<(?:[a-z]+|!--|\/)?)")]
        TagOpen,

        [Regex(@"((?:\/|--)?>)")]
        TagClose,

        //[Regex(@"([a-z]+)")]
        //TagName,

        [Regex(@"([a-z\s]+)")]
        Text,
    }
}