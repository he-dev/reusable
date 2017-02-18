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
        internal CommandLine(CommandCollection commands, char argumentPrefix, char nameValueSeparator, CommandInfo helpCommand, Action<string> log)
        {
            Commands = commands;
            ArgumentPrefix = argumentPrefix;
            ArgumentValueSeparator = nameValueSeparator;
            HelpCommand = helpCommand;
            Log = log;
        }

        public static CommandLineBuilder Builder => new CommandLineBuilder();

        private CommandInfo HelpCommand { get; }

        public char ArgumentPrefix { get; }

        public char ArgumentValueSeparator { get; }

        public CommandCollection Commands { get; }

        private Action<string> Log { get; }

        public void Execute(string commandLine)
        {
            var tokens = CommandLineTokenizer.Tokenize(commandLine ?? throw new ArgumentNullException(nameof(commandLine)), ArgumentValueSeparator);
            var arguments = CommandLineParser.Parse(tokens, ArgumentPrefix.ToString());

            if (arguments.CommandName == HelpCommand.Names)
            {
                HelpCommand.Instance.Execute(new CommandLineContext(this, new object(), Log));
            }
            else
            {
                var command = Commands[arguments.CommandName];
                if (command == null)
                {
                    // log
                }
                else
                {

                    command.Instance.Execute(new CommandLineContext(this, new object(), Log));
                }
            }
        }

        public void Execute(IEnumerable<string> args) => Execute(string.Join(" ", args ?? throw new ArgumentNullException(nameof(args))));

    }
}
