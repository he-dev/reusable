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
        private const string ParameterPrefix = @"^[-/\.]";

        private const string CommandSeparator = "|";

        public IEnumerable<ICommandLine> Parse(IEnumerable<string> tokens)
        {
            if (tokens == null) throw new ArgumentNullException(nameof(tokens));

            var commandLine = new CommandLine();
            var position = 1;
            var parameterId = Identifier.Command; // The first parameter is always a command.

            foreach (var token in tokens.Where(Conditional.IsNotNullOrEmpty))
            {
                switch (token)
                {
                    case CommandSeparator when commandLine.Any():
                        yield return commandLine;
                        commandLine = new CommandLine();
                        parameterId = Identifier.FromPosition(position++);
                        break;

                    case string value when IsParameterId(value):
                        parameterId = Identifier.FromName(RemoveParameterPrefix(value));
                        commandLine.Add(parameterId);
                        break;

                    default:
                        commandLine.Add(parameterId, token);
                        
                        // Use positional parameter-ids until a named one is found.
                        if (parameterId.Default.Option.Contains(NameOption.Positional))
                        {
                            parameterId = Identifier.FromPosition(position++);
                        }
                        break;
                }
            }

            if (commandLine.Any())
            {
                yield return commandLine;
            }

            bool IsParameterId(string value) => Regex.IsMatch(value, ParameterPrefix);

            string RemoveParameterPrefix(string value) => Regex.Replace(value, ParameterPrefix, string.Empty);
        }

        public IEnumerable<ICommandLine> Parse(string commandLine)
        {
            if (commandLine == null) throw new ArgumentNullException(nameof(commandLine));

            return Parse(_tokenizer.Tokenize(commandLine));
        }
    }
}