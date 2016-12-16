using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Fuse;
using Reusable.Shelly.Data;
using Reusable.Shelly.Reflection;

namespace Reusable.Shelly
{
    public class CommandLine
    {
        internal CommandLine(IEnumerable<CommandInfo> commands, string argumentPrefix, string argumentValueSeparator)
        {
            Commands = commands;
            ArgumentPrefix = argumentPrefix;
            ArgumentValueSeparator = argumentValueSeparator;
        }

        public string ArgumentPrefix { get; }

        public string ArgumentValueSeparator { get; }

        public IEnumerable<CommandInfo> Commands { get; }

        internal CommandFactory CommandFactory { get; } = new CommandFactory();

        internal CommandLineParser CommandLineParser { get; } = new CommandLineParser();

        public void Execute(IEnumerable<string> args)
        {
            args.Validate(nameof(args)).IsNotNull();

            var commandNames = Commands.SelectMany(x => x.CommandType.GetCommandNames());
            var parseResult = CommandLineParser.Parse(args, ArgumentPrefix, ArgumentValueSeparator, commandNames);
            var commandInfo = FindCommand(parseResult.CommandName);
            var command = CommandFactory.CreateCommand(commandInfo, parseResult.Arguments, this);
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
