using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Shelly.Data;
using Reusable.Shelly.Reflection;

namespace Reusable.Shelly
{
    public class CommandLineBuilder
    {
        private readonly List<CommandInfo> _commands = new List<CommandInfo>();
        private string _argumentPrefix = "-";
        private string _argumentValueSeparator = ":";

        //public CommandLineBuilder() { }

        public CommandLineBuilder ArgumentPrefix(string argumentPrefix)
        {
            _argumentPrefix = argumentPrefix;
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

            typeof(TCommand).ValidateCommandPropertyNames();

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
                throw new InvalidOperationException("There is already another default command.");
            }

            var cmd = _commands.LastOrDefault();
            if (cmd == null)
            {
                throw new InvalidOperationException("There need to be at least registered command in order to set it as default.");
            }

            cmd.IsDefault = true;

            return this;
        }

        public CommandLine Build()
        {
            return new CommandLine(_commands, _argumentPrefix, _argumentValueSeparator);
        }
    }
}