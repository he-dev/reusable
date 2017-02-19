using System;
using System.Windows.Input;
using Reusable;
using System.Collections.Generic;
using Reusable.Shelly.Collections;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;

namespace Reusable.Shelly.Data
{
    public class CommandInfo
    {
        public CommandInfo(ICommand command, StringSet names, CommandParameterCollection parameters)
        {
            Instance = command;
            Names = names;
            Parameters = parameters;
        }

        public CommandInfo(ICommand command)
            : this(
                  command, 
                  Command.GetNames(command.GetType()), null
            )
        { }

        public CommandInfo(ICommand command, StringSet names) 
            : this(
                  command, 
                  names, 
                  null
            )
        { }


        public CommandInfo(ICommand command, Type parameterType)
            : this(
                  command,
                  Command.GetNames(command.GetType()),
                  new CommandParameterCollection(parameterType)
            )
        { }

        public ICommand Instance { get; }

        public StringSet Names { get; }

        public CommandParameterCollection Parameters { get; }       
    }
}