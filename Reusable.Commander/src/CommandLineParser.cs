using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Commander
{
    public interface ICommandLineParser
    {
        [NotNull, ItemNotNull]
        IEnumerable<ICommandLine> Parse([NotNull, ItemCanBeNull] IEnumerable<string> commandLine);

        [NotNull, ItemNotNull]
        IEnumerable<ICommandLine> Parse([NotNull] string commandLine);
    }

    public class CommandLineParser : ICommandLineParser
    {
        private readonly ICommandLineTokenizer _tokenizer;

        public CommandLineParser([NotNull] ICommandLineTokenizer tokenizer)
        {
            _tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
        }

        // language=regexp
        private const string ArgumentPrefix = @"^[-/\.]";

        private const string CommandSeparator = "|";

        public IEnumerable<ICommandLine> Parse(IEnumerable<string> tokens)
        {
            if (tokens == null) throw new ArgumentNullException(nameof(tokens));

            var commandLine = new CommandLine();
            var argumentName = Identifier.Empty;

            foreach (var token in tokens.Where(Conditional.IsNotNullOrEmpty))
            {                
                switch (token)
                {
                    case CommandSeparator when commandLine.Any():
                        yield return commandLine;
                        commandLine = new CommandLine();
                        argumentName = Identifier.Empty;
                        break;

                    case string value when IsArgument(value):
                        argumentName = RemoveArgumentPrefix(token);
                        commandLine.Add(argumentName);
                        break;

                    default:
                        commandLine.Add(argumentName, token);
                        break;
                }
            }

            if (commandLine.Any())
            {
                yield return commandLine;
            }

            bool IsArgument(string value) => Regex.IsMatch(value, ArgumentPrefix);

            string RemoveArgumentPrefix(string value) => Regex.Replace(value, ArgumentPrefix, string.Empty);
        }

        public IEnumerable<ICommandLine> Parse(string commandLine)
        {
            if (commandLine == null) throw new ArgumentNullException(nameof(commandLine));

            return Parse(_tokenizer.Tokenize(commandLine));
        }
    }
}
