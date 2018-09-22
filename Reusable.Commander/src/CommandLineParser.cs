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
        IEnumerable<ICommandLine> Parse([NotNull, ItemNotNull] IEnumerable<string> commandLine);

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

        public IEnumerable<ICommandLine> Parse(IEnumerable<string> tokens)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }

            var commandLine = new CommandLine();
            var argumentName = CommandArgumentKeys.Anonymous;

            foreach (var token in tokens)
            {
                switch (token)
                {
                    case "|" when commandLine.Any():
                        yield return commandLine;
                        commandLine = new CommandLine();
                        argumentName = CommandArgumentKeys.Anonymous;
                        break;

                    // ReSharper disable once PatternAlwaysOfType
                    case string value when Regex.IsMatch(value, ArgumentPrefix):
                        argumentName = Regex.Replace(token, ArgumentPrefix, string.Empty);
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
        }

        public IEnumerable<ICommandLine> Parse(string commandLine)
        {
            if (commandLine == null)
            {
                throw new ArgumentNullException(nameof(commandLine));
            }

            return Parse(_tokenizer.Tokenize(commandLine));           
        }
    }
}
