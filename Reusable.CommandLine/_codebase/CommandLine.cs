using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Fuse;
using Reusable.Shelly.Reflection;

namespace Reusable.Shelly
{
    public partial class CommandLine
    {
        internal CommandLine(IEnumerable<CommandInfo> commands, string argumentPrefix, string argumentValueSeparator)
        {
            Commands = commands;
        }

        public string ArgumentPrefix { get; private set; }

        public string ArgumentValueSeparator { get; private set; }

        internal ILogger Logger { get; set; }

        public IEnumerable<CommandInfo> Commands { get; }

        internal CommandFactory CommandFactory { get; } = new CommandFactory();

        internal CommandLineParser CommandLineParser { get; } = new CommandLineParser();

        public void Execute(IEnumerable<string> args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            var containsCommand = false;
            var commandInfo = FindCommand(args, out containsCommand);

            if (containsCommand)
            {
                args = args.Skip(1);
            }

            try
            {
                var arguments = CommandLineParser.Parse(args, ArgumentPrefix, ArgumentValueSeparator);
                var command = CommandFactory.CreateCommand(commandInfo, arguments);
                command.CommandLine = this;

                try
                {
                    command.Execute();                    
                }
                catch (Exception ex)
                {
                    Logger.Error($"An error occured while executing '{command.GetType().Name}': {ex.Message}");
                    //return ExitCode.UnexpectedException;
                }
            }
            catch (CommandLineException ex)
            {
                Logger.Error(ex.Message);
                //return ex.ExitCode;
            }
            catch (Exception ex)
            {
                Logger.Error($"An unexpected error occured while creating command: {ex.Message}");
                //return ExitCode.UnexpectedException;
            }
        }

        internal CommandInfo FindCommand(IEnumerable<string> args, out bool containsCommand)
        {
            var arg0 = args.FirstOrDefault();

            if (string.IsNullOrEmpty(arg0))
            {
                var commandInfo = Commands.SingleOrDefault(x => x.IsDefault);
                if (commandInfo == null)
                {
                    // todo: throw CommandNotFoundException - there is no default command
                }
                containsCommand = false;
                return commandInfo;
            }
            else
            {
                var commandInfo = Commands.SingleOrDefault(x => x.CommandType.GetCommandNames().Contains(arg0, StringComparer.OrdinalIgnoreCase));
                if (commandInfo == null)
                {
                    // todo: throw CommandNotFoundException - there is not command "abc"
                }
                containsCommand = true;
                return commandInfo;
            }
        }

        public CommandInfo FindCommand(string name)
        {
            name.Validate(nameof(name)).IsNotNullOrEmpty();

            var commandInfo = Commands.SingleOrDefault(x => x.CommandType.GetCommandNames().Contains(name, StringComparer.OrdinalIgnoreCase));
            return commandInfo;
        }
    }
}
