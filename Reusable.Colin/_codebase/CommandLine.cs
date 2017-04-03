using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Collections.Immutable;
using System.Net.Http.Headers;
using Reusable.Colin.Collections;
using Reusable.Colin.Data;

namespace Reusable.Colin
{
    public class CommandLine
    {
        internal CommandLine(IEnumerable<ICommand> commands, char argumentPrefix, char nameValueSeparator, Action<string> log)
        {
            Commands = new CommandCollection(commands);
            ArgumentPrefix = argumentPrefix;
            ArgumentValueSeparator = nameValueSeparator;
            Log = log;
        }

        public static CommandLineBuilder Builder => new CommandLineBuilder();

        public char ArgumentPrefix { get; }

        public char ArgumentValueSeparator { get; }

        public CommandCollection Commands { get; }

        private Action<string> Log { get; }

        private ICommand DefaultCommand => Commands.TryGetCommand(Colin.Commands.DefaultCommand.NameSet, out ICommand command) ? command : default(ICommand);

        public void Execute(string commandLine)
        {
            var tokens = CommandLineTokenizer.Tokenize(commandLine ?? throw new ArgumentNullException(nameof(commandLine)), ArgumentValueSeparator);
            var argumentCollections = CommandLineParser.Parse(tokens, ArgumentPrefix.ToString());
            var commands = GetCommands(argumentCollections).ToList();

            if (commands.Any())
            {
                foreach (var item in commands) item.Command.Execute(item.Context);
            }
            else
            {
                DefaultCommand?.Execute(new CommandLineContext(this, new ArgumentCollection(), Log));
            }
        }

        private IEnumerable<(ICommand Command, CommandLineContext Context)> GetCommands(IEnumerable<ArgumentCollection> argumentCollections)
        {
            foreach (var arguments in argumentCollections)
            {
                if (Commands.TryGetCommand(arguments.CommandName, out ICommand command))
                {
                    yield return (command, new CommandLineContext(this, arguments, Log));
                }
                else
                {
                    yield return (DefaultCommand, new CommandLineContext(this, arguments, Log));
                }
            }
        }

        public void Execute(IEnumerable<string> args) => Execute(string.Join(" ", args ?? throw new ArgumentNullException(nameof(args))));
    }
}
