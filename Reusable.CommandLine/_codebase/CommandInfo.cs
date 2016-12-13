using System;

namespace Reusable.Shelly
{
    public class CommandInfo
    {
        private CommandInfo(Type commandType, object[] args)
        {
            CommandType = commandType;
            Args = args;
        }

        public Type CommandType { get; }

        public object[] Args { get; }

        public bool IsDefault { get; internal set; }

        internal static CommandInfo Create<TCommand>(object[] args) where TCommand : Command
        {
            return new CommandInfo(typeof(TCommand), args);
        }
    }
}