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

            switch (arguments.CommandName)
            {
                case StringSet names when names.Overlaps(HelpCommand.Names):
                    HelpCommand.Instance.Execute(new CommandLineContext(this, new object(), Log));
                    break;

                case StringSet names when Commands.TryGetCommand(names, out CommandInfo command):
                    command.Instance.Execute(new CommandLineContext(this, new object(), Log));
                    break;

                case null:
                    switch (CanUseImplicitMode())
                    {
                        case true:
                            Commands.Single().Instance.Execute(new CommandLineContext(this, new object(), Log));
                            break;

                        case false:
                            // Log("Invalid command name.")
                            break;
                    }
                    break;
            }

            bool CanUseImplicitMode() => Commands.Count == 1;
        }

        public void Execute(IEnumerable<string> args) => Execute(string.Join(" ", args ?? throw new ArgumentNullException(nameof(args))));

    }
}
