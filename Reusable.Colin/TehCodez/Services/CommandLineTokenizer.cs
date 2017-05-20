using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Colin.Services
{
    public static class CommandLineTokenizer
    {
        [NotNull]
        [ItemNotNull]
        [ContractAnnotation("text: null => halt")]
        public static IEnumerable<string> Tokenize([NotNull] this string text, char? nameValueSeparator = null)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));

            var escapableChars = new HashSet<char> { '\\', '"' };
            var separators = new HashSet<char> { ' ', '|' };
            //var tokens = new List<string>();
            var token = new StringBuilder();
            var escapeMode = false;
            var quoted = false;

            if (nameValueSeparator.HasValue)
            {
                escapableChars.Add(nameValueSeparator.Value);
                separators.Add(nameValueSeparator.Value);
            }

            bool IsUnquotedSeparator(char c) => separators.Contains(c) && !quoted;

            foreach (var c in text ?? throw new ArgumentNullException(nameof(text)))
            {
                switch (c)
                {
                    case '\\' when !quoted:
                        escapeMode = true;
                        // Don't eat escape-char yet.
                        break;

                    case '"':
                        quoted = !quoted;
                        // Don't eat quotes.
                        break;

                    default:

                        switch (escapeMode)
                        {
                            case true:
                                switch (!escapableChars.Contains(c))
                                {
                                    case true:
                                        token.Append(escapeMode);
                                        break;
                                }
                                token.Append(c);
                                escapeMode = false;
                                // Escape-char already eaten.
                                break;

                            default:
                                switch (IsUnquotedSeparator(c))
                                {
                                    case true when token.Length > 0:
                                        //tokens.Add(token.ToString());
                                        yield return token.ToString();
                                        token.Clear();
                                        switch (c)
                                        {
                                            // Pipe is a special token so it's collected.
                                            case '|':
                                                //tokens.Add(c.ToString());
                                                yield return c.ToString();
                                                break;
                                        }
                                        break;

                                    case true:
                                        // Don't eat separators.
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
                // tokens.Add(token.ToString());
                yield return token.ToString();
            }

            //return tokens;
        }
    }
}
