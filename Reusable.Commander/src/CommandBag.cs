using System.ComponentModel;
using Reusable.Commander.Services;

namespace Reusable.Commander
{
    public interface ICommandParameter
    {
        /// <summary>
        /// Specifies whether a command can be executed asynchronously.
        /// </summary>
        bool Async { get; set;  }
    }

    public class SimpleBag : ICommandParameter
    {
        //[DefaultValue(false)]
        //public bool CanThrow { get; set; }

        [DefaultValue(false)]
        public bool Async { get; set; }
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