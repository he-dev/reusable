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
                //SchemeSuffix,
                AuthorityPrefix,
                UserInfo,
                //UserInfoSuffix,
                Host,
                //PortPrefix,
                Port,
                PathPrefix,
                Path,
                //KeyPrefix,
                Key,
                //ValuePrefix,
                Value,
                //KeyPrefix,
                Key,
                //ValuePrefix,
                Value,
                //FragmentPrefix,
                Fragment
            };

            Assert.Equal(expectedTokens, tokens.Select(t => t.Type).ToArray());

            //var actual = string.Join("", tokens.Select(t => t.Text));

            Assert.Equal(new[] { "s", "//", "u", "h", "1", "/", "p", "k", "v", "k", "v", "f" }, tokens.Select(t => t.Text).ToArray());
        }

        [Theory]
        [InlineData("s://u@h:1/p?k=v&k=v#f", "s//uh1/pkvkvf")]
        [InlineData("s://u@h:1/p?k=v&k=v", "s//uh1/pkvkv")]
        [InlineData("s://u@h:1/p?k=v", "s//uh1/pkv")]
        [InlineData("s://u@h:1/p", "s//uh1/p")]
        [InlineData("s:///p", "s///p")]
        public void Can_tokenize_partial_URI(string uri, string expected)
        {
            // Using single letters for faster debugging.
            var tokens = UriStringTokenizer.Tokenize(uri).ToList();
            var actual = string.Join("", tokens.Select(t => t.Text));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Can_tokenize_authority_and_path()
        {
            var uri = "s:///p";
            var tokens = UriStringTokenizer.Tokenize(uri).ToList();
            //Assert.Equal(4, tokens.Count);
            //var actual = string.Join("", tokens.Select(t => t.Text));
            //Assert.Equal(uri, actual);
            Assert.Equal(new[] { "s", "//", "/", "p" }, tokens.Select(t => t.Text).ToArray());
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

    public class HtmlStringTokenizerTest
    {
        [Fact]
        public void Can_tokenize_html()
        {
            // Using single letters for faster debugging.
            var uri = "<p>t</p>";
            var tokens = HtmlStringTokenizer.Tokenize(uri).ToList();

//            var expectedTokens = new[]
//            {
//                Scheme,
//                SchemeSuffix,
//                AuthorityPrefix,
//                UserInfo,
//                UserInfoSuffix,
//                Host,
//                PortPrefix,
//                Port,
//                PathPrefix,
//                Path,
//                KeyPrefix,
//                Key,
//                ValuePrefix,
//                Value,
//                KeyPrefix,
//                Key,
//                ValuePrefix,
//                Value,
//                FragmentPrefix,
//                Fragment
//            };

            //Assert.Equal(expectedTokens, tokens.Select(t => t.Type).ToArray());

            var actual = string.Join("", tokens.Select(t => t.Text));

            Assert.Equal(uri, actual);
        }
    }

//    public static class Tokenizer
//    {
//        public static IEnumerable<Token<TToken>> Tokenize<TToken>(string value, IEnumerable<State<TToken>> states, Func<Token<TToken>> createToken)
//        {
//            states = states.ToList(); // Materialize states.
//
//            var state = states.First();
//            var token = createToken();
//            token.Type = state.Next;
//
//            foreach (var (oneChar, index) in value.Select((c, i) => (c.ToString(), i)))
//            {
//                // The state matches itself.
//                if (state.IsMatch(oneChar) is var m && m.Success && !m.IsPartial)
//                {
//                    token.Text.Append(oneChar);
//                }
//                else
//                {
//                    yield return token;
//                    var isMatch = false;
//                    // Find states where the current one is `Prev`.
//                    foreach (var next in states.Where(s => s.Prev.Equals(token.Type)))
//                    {
//                        // There is a match. Use this state from now on.
//                        if ((isMatch = next.IsMatch(oneChar, out _)))
//                        {
//                            // Initialize the new token.
//                            token = createToken();
//                            token.StartIndex = index;
//                            token.Type = next.Next;
//                            token.Text.Append(oneChar);
//                            state = next;
//                            // Got to the next character.
//                            break;
//                        }
//                    }
//
//                    // There was no match. This means the current char is invalid.
//                    if (!isMatch)
//                    {
//                        throw new ArgumentException($"Invalid character at: {index}.");
//                    }
//                }
//            }
//
//            // Yield the last token.
//            if (token.Text.Length > 0)
//            {
//                yield return token;
//            }
//        }
//    }

    public static class Tokenizer2
    {
        public static IEnumerable<Token<TToken>> Tokenize<TToken>(string value, IEnumerable<State<TToken>> states, Func<Token<TToken>> createToken)
        {
            states = states.ToList(); // Materialize states.

            var current = states.Take(1).ToList();

            var startIndex = 0;
            var token = new StringBuilder();

            for (var i = 0; i < value.Length; i++)
            {
                // The state matches itself.
                token.Append(value[i]);

                if (current.Any(s => s.Matcher.IsMatch(token.ToString())))
                {
                    continue;
                }
                else
                {
                    // Backtrack. Undo the last character.
                    token.Length--;

                    var match = current.Single(s => s.Matcher.IsMatch(token.ToString()));

                    // Yield the current token.
                    var t = createToken();
                    t.StartIndex = startIndex;
                    t.Text = match.Matcher.GetValue(token.ToString());
                    t.Type = match.Next;
                    yield return t;

                    // Initialize a new token.
                    startIndex = i;
                    token = new StringBuilder().Append(value[i]);

                    // Find states where the current one is `Prev`.
                    var next = states.Where(s => (!s.IsToSelf || s.Matcher.AllowConsecutive) && s.Prev.Equals(match.Next) && s.Matcher.IsMatch(token.ToString())).ToList();
                    if (next.Any())
                    {
                        // Got to the next character.
                        current = next;
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid character at: {i}.");
                    }
                }
            }

            // Yield the last token.
            if (token.Length > 0)
            {
                var match = current.Single(s => s.Matcher.IsMatch(token.ToString()));

                var t = createToken();
                t.StartIndex = startIndex;
                t.Text = match.Matcher.GetValue(token.ToString());
                t.Type = match.Next;
                yield return t;
            }
        }
    }

    public abstract class MatcherAttribute : Attribute
    {
        public bool AllowConsecutive { get; set; }

        public abstract bool IsMatch(string value);

        public abstract string GetValue(string value);
    }

    public class PatternAttribute : MatcherAttribute
    {
        private readonly string _pattern;
        public PatternAttribute([RegexPattern] string pattern) => _pattern = $"^{pattern}$";

        public override bool IsMatch(string value)
        {
            return Regex.IsMatch(value, _pattern);
        }

        public override string GetValue(string value)
        {
            return Regex.Match(value, _pattern).Groups[1].Value;
        }
    }

    public class ExactAttribute : MatcherAttribute
    {
        private readonly string _pattern;
        public ExactAttribute(string pattern) => _pattern = pattern;

        public override bool IsMatch(string value)
        {
            if (value.Length > _pattern.Length) return false;

            var matchCount = 0;

            for (var i = 0; i < value.Length; i++)
            {
                if (!value[i].Equals(_pattern[i]))
                {
                    break;
                }

                matchCount++;
            }

            return matchCount > 0;
        }

        public override string GetValue(string value)
        {
            return _pattern;
        }
    }

    public class State<TToken>
    {
        public State(TToken prev, TToken next)
        {
            Prev = prev;
            Next = next;
            Matcher = typeof(TToken).GetField(Next.ToString()).GetCustomAttribute<MatcherAttribute>();
        }

        public TToken Prev { get; set; }

        public TToken Next { get; set; }

        public bool IsToSelf => Prev.Equals(Next);

        public MatcherAttribute Matcher { get; }

        public override string ToString() => $"<-- {Prev} | {Next} -->";
    }

    public class Token<TToken>
    {
        public int StartIndex { get; set; }

        public string Text { get; set; }

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

            //(Scheme, SchemeSuffix),
            //(SchemeSuffix, Path),
            //(SchemeSuffix, AuthorityPrefix),
            (AuthorityPrefix, UserInfo),
            (AuthorityPrefix, Host),
            (AuthorityPrefix, PathPrefix),
            //(UserInfo, UserInfoSuffix),
            //(UserInfoSuffix, Host),
            (Host, PathPrefix),
            //(Host, PortPrefix),
            //(PortPrefix, Port),
            (Port, PathPrefix),
            (PathPrefix, Path),
            //(Path, KeyPrefix),
            //(KeyPrefix, Key),
            //(Key, ValuePrefix),
            //(ValuePrefix, Value),
            //(Value, KeyPrefix),
            //(Key, FragmentPrefix),
            //(Value, FragmentPrefix),
            //(FragmentPrefix, Fragment)

            (UserInfo, Host),
            (Host, Port),
            (Scheme, AuthorityPrefix),
            (Path, Key),
            (Key, Value),
            (Value, Key),
            (Path, Fragment),
            (Key, Fragment),
            (Value, Fragment),

            // --
        }.Select(t => new State<UriToken>(t.Prev, t.Next)).ToList();

        public static IEnumerable<Token<UriToken>> Tokenize(string value)
        {
            return Tokenizer2.Tokenize(value, States, () => new Token<UriToken>());
        }
    }

    public enum UriToken
    {
        [Pattern(@"([a-z]+):?")]
        Scheme,

//        [Pattern(@":")]
//        SchemeSuffix,

        //[Pattern(@"\/")]
        [Exact("//")]
        AuthorityPrefix,

        [Pattern(@"([a-z]+)@?")]
        UserInfo,

//        [Exact(@"@")]
//        UserInfoSuffix,

        [Pattern(@"([a-z]+)")]
        Host,

//        [Exact(@":")]
//        PortPrefix,

        [Pattern(@":([0-9]*)")]
        Port,

        [Exact(@"/")]
        PathPrefix,

        [Pattern(@"([a-z]+)")]
        Path,

        //QueryPrefix,

//        [Pattern(@"[\?\&]")]
//        KeyPrefix,

        [Pattern(@"[\?\&]([a-z]*)", AllowConsecutive = true)]
        Key,

//        [Exact(@"=")]
//        ValuePrefix,

        [Pattern(@"=([a-z]*)")]
        Value,

//        [Exact(@"#")]
//        FragmentPrefix,

        [Pattern(@"#([a-z]*)")]
        Fragment,
    }

    public static class HtmlStringTokenizer
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

        public static readonly ICollection<State<HtmlToken>> States = new (HtmlToken Prev, HtmlToken Next)[]
        {
            // self

            (TagOpen, TagOpen),
            (TagClose, TagClose),
            (ClosingTagOpen, ClosingTagOpen),
            (TagName, TagName),
            (Text, Text),


            // transitions

            (TagOpen, TagName),
            (TagName, TagClose),
            (TagName, ClosingTagOpen),
            (TagClose, Text),
            (Text, TagOpen),
            (Text, ClosingTagOpen),
            (ClosingTagOpen, Text),
            (Text, TagClose),


            // --
        }.Select(t => new State<HtmlToken>(t.Prev, t.Next)).ToList();

        public static IEnumerable<Token<HtmlToken>> Tokenize(string value)
        {
            return Tokenizer2.Tokenize(value, States, () => new Token<HtmlToken>());
        }
    }

    public enum HtmlToken
    {
        [Exact(@"<")]
        TagOpen,

        [Exact(@">")]
        TagClose,

        [Exact(@"</")]
        ClosingTagOpen,

        [Pattern(@"([a-z]+)")]
        TagName,

        [Pattern(@"([a-z]+)")]
        Text,
    }
}