using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Reusable.Converters;
using Reusable.Fuse;
using Reusable.Fuse.Testing;
using Reusable.Shelly.Collections;
using Reusable.Shelly.Data;
using Reusable.Shelly.Reflection;

namespace Reusable.Shelly
{
    internal class CommandFactory
    {
        

        internal CommandFactory() { }

        public Command CreateCommand(CommandInfo commandInfo, CommandLine commandLine)
        {
            commandInfo.Validate(nameof(commandInfo)).IsNotNull();
            commandLine.Validate(nameof(commandLine)).IsNotNull();

            // todo: implement with Autofac
            var requriesCommandLine = commandInfo.CommandType.GetConstructor(new[] { typeof(CommandLine) }.Concat(commandInfo.Args.Select(x => x.GetType())).ToArray()) != null;

            var command =
                requriesCommandLine
                ? (Command)Activator.CreateInstance(commandInfo.CommandType, new object[] { commandLine }.Concat(commandInfo.Args).ToArray())
                : (Command)Activator.CreateInstance(commandInfo.CommandType, commandInfo.Args);

            //PopulateProperties(command, arguments);
            return command;
        }
    }
}