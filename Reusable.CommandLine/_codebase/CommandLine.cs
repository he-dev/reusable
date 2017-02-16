using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Fuse;
using Reusable.Shelly.Data;
using Reusable.Shelly.Reflection;
using Reusable.Shelly.Collections;

namespace Reusable.Shelly
{
    public class CommandLine
    {
        internal CommandLine(CommandCollection commands, string argumentPrefix, char nameValueSeparator)
        {
            Commands = commands;
            ArgumentPrefix = argumentPrefix;
            NameValueSeparator = nameValueSeparator;
        }

        public string ArgumentPrefix { get; }

        public char NameValueSeparator { get; }

        public CommandCollection Commands { get; }

        public void Execute(IEnumerable<string> args)
        {
            args.Validate(nameof(args)).IsNotNull();

            var commandNames = Commands.SelectMany(x => x.CommandType.GetCommandNames()).ToList();

            var tokens = CommandLineTokenizer.Tokenize(string.Join(" ", args), NameValueSeparator);
            var arguments = CommandLineParser.Parse(tokens, ArgumentPrefix);
            var commandInfo = FindCommand(arguments.CommandName);
            var command = CommandFactory.CreateCommand(commandInfo, this);
            command.Execute();
        }

        public CommandInfo FindCommand(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Commands
                    .FirstOrDefault(x => x.IsDefault)
                    .Validate()
                    .Throws(typeof(CommnadNotFoundException))
                    .IsNotNull("Default command not found.").Value;
            }

            return Commands
                .SingleOrDefault(x => x.CommandType.GetCommandNames().Contains(name, StringComparer.OrdinalIgnoreCase))
                .Validate()
                .Throws(typeof(CommnadNotFoundException))
                .IsNotNull($"Command \"{name}\" not found.").Value;
        }
    }

   
}
