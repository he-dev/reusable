using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Shelly.Commands;
using Reusable.Shelly.Reflection;
using Reusable.Shelly.Writers;

namespace Reusable.Shelly
{
    public class CommandLineBuilder
    {
        private readonly List<CommandInfo> _commands = new List<CommandInfo>();
        private string _argznentPrefix = "-";
        private string _argumentValueSeparator = ":";
#if DEBUG
        private ILogger _logger = Shelly.Logger.Empty.Add<ConsoleLogger>(LogLevel.Trace).Add<DebugLogger>(LogLevel.Trace);
#else
            private ILogger _logger = Candle.Logger.Empty.Add<ConsoleLogger>().Add<DebugLogger>();
#endif

        public CommandLineBuilder()
        {
        }

        public CommandLineBuilder ArgumentPrefix(string argumentPrefix)
        {
            _argznentPrefix = argumentPrefix;
            return this;
        }

        public CommandLineBuilder ArgumentValueSeparator(string argumentValueSeparator)
        {
            _argumentValueSeparator = argumentValueSeparator;
            return this;
        }

        public CommandLineBuilder Register<TCommand>(params object[] args) where TCommand : Command
        {
            var commandNames = _commands.SelectMany(x => x.CommandType.GetCommandNames());
            var nameCollision = typeof(TCommand).GetCommandNames().FirstOrDefault(name => commandNames.Contains(name, StringComparer.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(nameCollision))
            {
                throw new ArgumentException($"Command '{typeof(TCommand).FullName}' cannot be added because there is already another command with this name: \"{nameCollision}\".");
            }

            typeof(TCommand).ValidateCommandPropertyNamesAreUnique();

            _commands.Add(CommandInfo.Create<TCommand>(args));
            return this;
        }

        public CommandLineBuilder Register<TCommand>() where TCommand : Command, new()
        {
            Register<TCommand>(null);
            return this;
        }

        public CommandLineBuilder AsDefault()
        {
            if (_commands.Any(x => x.IsDefault))
            {
                // todo: throw DefaultCommandException
            }
            var cmd = _commands.LastOrDefault();
            if (cmd == null)
            {
                // todo: throw CommandNotRegisteredException
            }

            cmd.IsDefault = true;

            return this;
        }

        public CommandLineBuilder Logger(ILogger logger)
        {
            _logger = logger;
            return this;
        }

        public CommandLine Build()
        {
            return new CommandLine(_commands, _argznentPrefix, _argumentValueSeparator);
        }
    }

    public static class CommandLineBuilderExtensions
    {
        public static CommandLineBuilder RegisterHelpCommand(this CommandLineBuilder builder, IHelpWriter helpWriter)
        {
            return builder.Register<HelpCommand>(helpWriter);
        }
    }
}