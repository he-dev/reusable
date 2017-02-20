using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Fuse;
using Reusable.Shelly.Data;
using Reusable.Shelly.Reflection;
using Reusable.Shelly.Collections;
using System.Windows.Input;

namespace Reusable.Shelly
{
    public class CommandLine
    {
        internal CommandLine(CommandCollection commands, char argumentPrefix, char nameValueSeparator, Action<string> log)
        {
            Commands = commands;
            ArgumentPrefix = argumentPrefix;
            ArgumentValueSeparator = nameValueSeparator;
            Log = log;
        }

        public static CommandLineBuilder Builder => new CommandLineBuilder();

        public char ArgumentPrefix { get; }

        public char ArgumentValueSeparator { get; }

        public CommandCollection Commands { get; }

        private Action<string> Log { get; }

        public void Execute(string commandLine)
        {
            var tokens = CommandLineTokenizer.Tokenize(commandLine ?? throw new ArgumentNullException(nameof(commandLine)), ArgumentValueSeparator);
            var arguments = CommandLineParser.Parse(tokens, ArgumentPrefix.ToString());
            var context = new CommandLineContext(this, arguments, Log);

            switch (arguments.CommandName)
            {
                case StringSet nameSet when Commands.TryGetCommand(nameSet, out ICommand command):
                    command.Execute(context);
                    break;

                case null when Commands.TryGetCommand(DefaultCommand.NameSet, out ICommand command):
                    command.Execute(context);
                    break;

                case null:
                    // Log("Invalid command name.")
                    break;
            }
        }

        public void Execute(IEnumerable<string> args) => Execute(string.Join(" ", args ?? throw new ArgumentNullException(nameof(args))));
    }
}
