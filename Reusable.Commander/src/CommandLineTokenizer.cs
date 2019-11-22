using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Commander
{
    public interface ICommandLineTokenizer
    {
        [NotNull, ItemNotNull]
        IEnumerable<string> Tokenize([CanBeNull] string text);
    }

    public class CommandLineTokenizer : ICommandLineTokenizer
    {
        public static readonly IImmutableSet<char> Separators = new[] { ' ', '|', ',', ':', '=' }.ToImmutableHashSet();
        public static readonly IImmutableSet<char> Escapables = new[] { '\\', '"' }.Concat(Separators).ToImmutableHashSet();

        public IEnumerable<string> Tokenize(string text)
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
                                            case '|': yield return c.ToString(); break;
                                        }
                                        break;

                                    case true:
                                        // Don't eat separators...
                                        switch (c)
                                        {
                                            // ...unless it's a pipe.
                                            case '|': yield return c.ToString(); break;
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
}
