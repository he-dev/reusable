using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Shelly.Data;
using Reusable.Shelly.Reflection;
using Reusable.Shelly.Collections;
using System.Windows.Input;

namespace Reusable.Shelly
{
    public class CommandLineBuilder
    {
        private readonly CommandCollection _commands = new CommandCollection();
        private string _argumentPrefix = "-";
        private string _argumentValueSeparator = ":";

        internal CommandLineBuilder() { }

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

        public CommandLineBuilder Register<TCommand>() where TCommand : ICommand, new()
        {
            _commands.Add(new TCommand());
            return this;
        }

        public CommandLine Build()
        {
            return new CommandLine(_commands, _argumentPrefix, _argumentValueSeparator);
        }
    }
}