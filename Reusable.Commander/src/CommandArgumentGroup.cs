using System.ComponentModel;

namespace Reusable.Commander
{
    public interface ICommandArgumentGroup
    {
        /// <summary>
        /// Specifies whether a command can be executed asynchronously.
        /// </summary>
        bool Async { get; set;  }
    }

    public enum CommandExecutionType
    {
        Sequential,
        Asynchronous
    }

    public static class CommandBagExtensions
    {
        internal static CommandExecutionType ExecutionType(this Executable executable)
        {
            return
                executable.Async
                    ? CommandExecutionType.Asynchronous
                    : CommandExecutionType.Sequential;
        }
    }
}