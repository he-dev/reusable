using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Reusable.Extensions;
using Reusable.Lexing;
using Reusable.Lexing.Matchers;

namespace Reusable.Commander
{
    using static CommandLineToken;

    public interface ICommandLineTokenizer
    {
        IEnumerable<string> Tokenize(string? text);
    }

    public class CommandLineTokenizer : ICommandLineTokenizer
    {
        public static readonly IImmutableSet<char> Separators = new[] { ' ', '|', ',', ':', '=' }.ToImmutableHashSet();
        public static readonly IImmutableSet<char> Escapables = new[] { '\\', '"' }.Concat(Separators).ToImmutableHashSet();

        public IEnumerable<string> Tokenize(string? text)
        {
            if (text.IsNullOrEmpty())
            {
                yield break;
            }

            var token = new StringBuilder();
            var escaped = false;
            var quoted = false;

            bool IsUnquotedSeparator(char c) => Separators.Contains(c) && !quoted;

            foreach (var c in text)
            {
                switch (c)
                {
                    case '\\' when !quoted && !escaped:
                        escaped = true;
                        // Don't eat escape-char yet.
                        break;

                    case '"':
                        quoted = !quoted;
                        // Don't eat quotes.
                        break;

                    default:

                        switch (escaped)
                        {
                            case true:
                                switch (!Escapables.Contains(c))
                                {
                                    case true:
                                        // Eat escape-char because it doesn't escape any valid char.
                                        token.Append("\\");
                                        break;
                                }

                                token.Append(c);
                                escaped = false;
                                break;

                            default:
                                switch (IsUnquotedSeparator(c))
                                {
                                    case true when token.Any():
                                        yield return token.ToString();
                                        token.Clear();
                                        switch (c)
                                        {
                                            // Pipe is a special separator that is treated like a token.
                                            case '|':
                                                yield return c.ToString();
                                                break;
                                        }

                                        break;

                                    case true:
                                        // Don't eat separators...
                                        switch (c)
                                        {
                                            // ...unless it's a pipe.
                                            case '|':
                                                yield return c.ToString();
                                                break;
                                        }

                                        break;

                                    default:
                                        token.Append(c);
                                        break;
                                }

                                break;
                        }

                        break;
                }
            }

            if (token.Any())
            {
                yield return token.ToString();
            }
        }
    }
    
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

    public class CommandLineTokenizer2 : Tokenizer<CommandLineToken>
    {
        /*
                           
          input ------ x ------------- x ----- x ---> command-line
                \     / \             / \     /
                 value   --arg ----- /   -flag
                              \     /
                               value                                                  
        */

        public CommandLineTokenizer2() : base(new StateTransitionBuilder<CommandLineToken>
        {
            { default, Value },
            { Value, Value, Argument, Flag },
            { Argument, Argument, Value, Flag },
            { Flag, Flag, Argument }
        }) { }
    }
}