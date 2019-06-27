using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Commander
{
    public class CommandLineDictionary : Dictionary<Identifier, CommandArgument> { }

    public interface ICommandLineParser
    {
        [NotNull, ItemNotNull]
        IEnumerable<CommandLineDictionary> Parse([NotNull, ItemCanBeNull] IEnumerable<string> commandLine);

        [NotNull, ItemNotNull]
        IEnumerable<CommandLineDictionary> Parse([NotNull] string commandLine);
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

        public IEnumerable<CommandLineDictionary> Parse(IEnumerable<string> tokens)
        {
            if (tokens == null) throw new ArgumentNullException(nameof(tokens));

            var position = 1;
            var argumentName = Identifier.Command; // The first parameter is always a command.
            var commandLine = NewCommandLineDictionary(argumentName);

            foreach (var token in tokens.Where(Conditional.IsNotNullOrEmpty))
            {
                switch (token)
                {
                    case CommandSeparator when commandLine.Any():
                        yield return commandLine;
                        position = 1;
                        argumentName = Identifier.Command;
                        commandLine = NewCommandLineDictionary(argumentName);
                        break;

                    case string value when IsArgumentName(value):
                        argumentName = Identifier.FromName(RemoveParameterPrefix(value));
                        commandLine.Add(argumentName, new CommandArgument(argumentName, Enumerable.Empty<string>()));
                        break;

                    default:
                        commandLine[argumentName].Add(token);

                        // Use positional parameter-ids until a named one is found.
                        if (argumentName.Default.Option.Contains(NameOption.Positional))
                        {
                            argumentName = Identifier.FromPosition(position++);
                        }

                        break;
                }
            }

            if (commandLine.Any())
            {
                yield return commandLine;
            }

            bool IsArgumentName(string value) => Regex.IsMatch(value, ParameterPrefix);

            string RemoveParameterPrefix(string value) => Regex.Replace(value, ParameterPrefix, string.Empty);
        }

        public IEnumerable<CommandLineDictionary> Parse(string commandLine)
        {
            if (commandLine == null) throw new ArgumentNullException(nameof(commandLine));

            return Parse(_tokenizer.Tokenize(commandLine));
        }

        private static CommandLineDictionary NewCommandLineDictionary(Identifier commandName)
        {
            return new CommandLineDictionary { [commandName] = new CommandArgument(commandName, Enumerable.Empty<string>()) };
        }
    }
}