using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Xunit;

namespace Reusable.Experimental
{
    using static UriToken;
    
    public class UriStringParserTest
    {
        public static class Patterns
        {
            // language=regex
            public const string Scheme = "[a-z]";

            // language=regex
            public const string SchemeSuffix = ":";

            // language=regex
            public const string Path = "[a-z]";
        }

        [Fact]
        public void Can_tokenize_full_URI()
        {
            // Using single letters for faster debugging.
            var uri = "s://u@h:1/p?k=v&k=v#f";
            var tokens = UriStringTokenizer.Tokenize(uri).ToList();

            var expectedTokens = new[]
            {
                Scheme,
                SchemeSuffix,
                AuthorityPrefix,
                UserInfo,
                UserInfoSuffix,
                Host,
                PortPrefix,
                Port,
                PathPrefix,
                Path,
                KeyPrefix,
                Key,
                ValuePrefix,
                Value,
                KeyPrefix,
                Key,
                ValuePrefix,
                Value,
                FragmentPrefix,
                Fragment
            };

            Assert.Equal(expectedTokens, tokens.Select(t => t.Type).ToArray());

            var actual = string.Join("", tokens.Select(t => t.Text));

            Assert.Equal(uri, actual);
        }

        [Theory]
        [InlineData("s://u@h:1/p?k=v&k=v#f")]
        [InlineData("s://u@h:1/p?k=v&k=v")]
        [InlineData("s://u@h:1/p?k=v")]
        [InlineData("s://u@h:1/p")]
        [InlineData("s:///p")]
        public void Can_tokenize_partial_URI(string uri)
        {
            // Using single letters for faster debugging.
            var tokens = UriStringTokenizer.Tokenize(uri).ToList();
            var actual = string.Join("", tokens.Select(t => t.Text));
            Assert.Equal(uri, actual);
        }

        [Fact]
        public void Throws_when_invalid_character()
        {
            // Using single letters for faster debugging.
            var uri = "s://:u@h:1/p?k=v&k=v#f";
            //             ^ - invalid character

            var ex = Assert.Throws<ArgumentException>(() => UriStringTokenizer.Tokenize(uri).ToList());
            Assert.Equal("Invalid character at: 4.", ex.Message);
        }
    }

    public static class Tokenizer
    {
        public static IEnumerable<Token<TToken>> Tokenize<TToken>(string value, IEnumerable<State<TToken>> states, Func<Token<TToken>> createToken)
        {
            states = states.ToList(); // Materialize states.

            var state = states.First();
            var token = createToken();
            token.Type = state.Next;

            foreach (var (oneChar, index) in value.Select((c, i) => (c.ToString(), i)))
            {
                // The state matches itself.
                if (state.IsMatch(oneChar))
                {
                    token.Text.Append(oneChar);
                }
                else
                {
                    yield return token;
                    var isMatch = false;
                    // Find states where the current one is `Prev`.
                    foreach (var next in states.Where(s => s.Prev.Equals(token.Type)))
                    {
                        // There is a match. Use this state from now on.
                        if ((isMatch = next.IsMatch(oneChar)))
                        {
                            // Initialize the new token.
                            token = createToken();
                            token.StartIndex = index;
                            token.Type = next.Next;
                            token.Text.Append(oneChar);
                            state = next;
                            // Got to the next character.
                            break;
                        }
                    }

                    // There was no match. This means the current char is invalid.
                    if (!isMatch)
                    {
                        throw new ArgumentException($"Invalid character at: {index}.");
                    }
                }
            }
            
            // Yield the last token.
            if (token.Text.Length > 0)
            {
                yield return token;
            }
        }
    }

    public class PatternAttribute : Attribute
    {
        private readonly string _pattern;
        public PatternAttribute([RegexPattern] string pattern) => _pattern = pattern;
        public bool IsMatch(string value) => Regex.IsMatch(value, _pattern);
    }

    public class State<TToken>
    {
        public TToken Prev { get; set; }

        public TToken Next { get; set; }

        public bool IsMatch(string value)
        {
            return
                typeof(TToken)
                    .GetField(Next.ToString())
                    .GetCustomAttribute<PatternAttribute>()
                    .IsMatch(value);
        }

        public override string ToString() => $"<-- {Prev} | {Next} -->";
    }

    public class Token<TToken>
    {
        public int StartIndex { get; set; }

        public StringBuilder Text { get; set; } = new StringBuilder();

        public TToken Type { get; set; }

        public override string ToString() => $"{StartIndex}: {Text} ({Type})";
    }

    public static class UriStringTokenizer
    {
        /*
         
         scheme:[//[userinfo@]host[:port]]path[?key=value&key=value][#fragment]
                [ ----- authority ----- ]     [ ----- query ------ ]
          
         scheme: ------------------------- path -------------------------  --------- UriString
                \                         /    \                         /\         /
                 // --------- host ---- '/'     ?key ------ &key ------ /  #fragment
                   \         /    \     /           \      /    \      /
                    userinfo@      :port             =value      =value             
          
        */

        public static readonly ICollection<State<UriToken>> States = new (UriToken Prev, UriToken Next)[]
        {
            // self

            (Scheme, Scheme),
            (UserInfo, UserInfo),
            (Host, Host),
            (Port, Port),
            (Path, Path),
            (Key, Key),
            (Value, Value),
            (Fragment, Fragment),

            // transitions

            (Scheme, SchemeSuffix),
            (SchemeSuffix, Path),
            (SchemeSuffix, AuthorityPrefix),
            (AuthorityPrefix, UserInfo),
            (AuthorityPrefix, Host),
            (UserInfo, UserInfoSuffix),
            (UserInfoSuffix, Host),
            (Host, PathPrefix),
            (Host, PortPrefix),
            (PortPrefix, Port),
            (Port, PathPrefix),
            (PathPrefix, Path),
            (Path, KeyPrefix),
            (KeyPrefix, Key),
            (Key, ValuePrefix),
            (ValuePrefix, Value),
            (Value, KeyPrefix),
            (Key, FragmentPrefix),
            (Value, FragmentPrefix),
            (FragmentPrefix, Fragment)

            // --
        }.Select(t => new State<UriToken> { Prev = t.Prev, Next = t.Next, }).ToList();

        public static IEnumerable<Token<UriToken>> Tokenize(string value)
        {
            return Tokenizer.Tokenize(value, States, () => new Token<UriToken>());
        }
    }

    public enum UriToken
    {
        [Pattern(@"[a-z]")]
        Scheme,

        [Pattern(@":")]
        SchemeSuffix,

        [Pattern(@"\/")]
        AuthorityPrefix,

        [Pattern(@"[a-z]")]
        UserInfo,

        [Pattern(@"@")]
        UserInfoSuffix,

        [Pattern(@"[a-z]")]
        Host,

        [Pattern(@":")]
        PortPrefix,

        [Pattern(@"[0-9]")]
        Port,

        [Pattern(@"\/")]
        PathPrefix,

        [Pattern(@"[a-z]")]
        Path,

        //QueryPrefix,

        [Pattern(@"[\?\&]")]
        KeyPrefix,

        [Pattern(@"[a-z]")]
        Key,

        [Pattern(@"=")]
        ValuePrefix,

        [Pattern(@"[a-z]")]
        Value,

        [Pattern(@"#")]
        FragmentPrefix,

        [Pattern(@"[a-z]")]
        Fragment,
    }
}