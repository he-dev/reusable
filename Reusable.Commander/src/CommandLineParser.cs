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
        IEnumerable<List<CommandLineArgument>> Parse(string commandLine);
    }

    public class CommandLineParser : ICommandLineParser
    {
        private readonly ICommandLineTokenizer _tokenizer;

        public CommandLineParser(ICommandLineTokenizer tokenizer)
        {
            _tokenizer = tokenizer;
        }

        // language=regexp
        private const string ParameterPrefix = @"^[-/\.]";

        private const string CommandSeparator = "|";

        public IEnumerable<List<CommandLineArgument>> Parse(string commandLine)
        {
            // The first parameter is always a command.
            var arguments = new List<CommandLineArgument> { new CommandLineArgument(MultiName.Command) };

            foreach (var token in _tokenizer.Tokenize(commandLine).Where(Conditional.IsNotNullOrEmpty))
            {
                switch (token)
                {
                    case CommandSeparator when arguments.Any():
                        yield return arguments;
                        arguments = new List<CommandLineArgument> { new CommandLineArgument(MultiName.Command) };
                        break;

                    case { } value when IsArgumentName(value):
                        arguments.Add(new CommandLineArgument(RemoveArgumentPrefix(value)));
                        break;

                    default:
                        arguments.Last().Add(token);
                        break;
                }
            }

            if (arguments.Any())
            {
                yield return arguments;
            }

            bool IsArgumentName(string value) => Regex.IsMatch(value, ParameterPrefix);

            string RemoveArgumentPrefix(string value) => Regex.Replace(value, ParameterPrefix, string.Empty);
        }
    }
}