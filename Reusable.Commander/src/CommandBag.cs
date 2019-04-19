using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

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
        internal static CommandExecutionType ExecutionType(this ICommandParameter bag)
        {
            return
                bag.Async
                    ? CommandExecutionType.Asynchronous
                    : CommandExecutionType.Sequential;
        }
    }
}