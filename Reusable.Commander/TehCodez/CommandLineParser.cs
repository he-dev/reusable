using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Extensions;
using SoftKeySet = Reusable.Collections.ImmutableKeySet<Reusable.SoftString>;

namespace Reusable.Commander
{
    public interface ICommandLineParser
    {
        [NotNull, ItemNotNull]
        IEnumerable<CommandLine> Parse([CanBeNull] string commandLine);
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

        public IEnumerable<CommandLine> Parse(string commandLine)
        {
            if (commandLine.IsNullOrEmpty())
            {
                yield break;
            }

            var tokens = _tokenizer.Tokenize(commandLine);

            var arguments = CommandLine.Empty;
            var currentArgumentName = SoftKeySet.Empty;

            foreach (var token in tokens)
            {
                switch (token)
                {
                    case "|" when arguments.Any():
                        yield return arguments;
                        arguments = new CommandLine();
                        break;

                    // ReSharper disable once PatternAlwaysOfType
                    case string value when Regex.IsMatch(value, ArgumentPrefix):
                        currentArgumentName = SoftKeySet.Create(Regex.Replace(token, ArgumentPrefix, string.Empty));
                        arguments.Add(currentArgumentName);
                        break;

                    default:
                        arguments.Add(currentArgumentName, token);
                        break;
                }
            }

            if (arguments.Any())
            {
                yield return arguments;
            }
        }
    }
}
