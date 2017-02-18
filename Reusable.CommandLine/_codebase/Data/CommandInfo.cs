using System;
using System.Windows.Input;
using Reusable;
using System.Collections.Generic;
using Reusable.Shelly.Collections;

namespace Reusable.Shelly.Data
{
    public class CommandInfo
    {
        public CommandInfo(ICommand command, StringSetCI names, CommandParameterCollection parameters)
        {
            Instance = command;
            Names = names;
            Parameters = parameters;
        }

        public CommandInfo(ICommand command)
            : this(
                  command, 
                  CommandReflection.GetCommandNames(command.GetType()), null
            )
        { }

        public CommandInfo(ICommand command, StringSetCI names) 
            : this(
                  command, 
                  names, 
                  null
            )
        { }


        public CommandInfo(ICommand command, Type parameterType)
            : this(
                  command,
                  CommandReflection.GetCommandNames(command.GetType()),
                  new CommandParameterCollection(parameterType)
            )
        { }

        public ICommand Instance { get; }

        public StringSetCI Names { get; }

        public CommandParameterCollection Parameters { get; }
    }
}