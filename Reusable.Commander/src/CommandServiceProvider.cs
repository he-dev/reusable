using Reusable.OmniLog;

namespace Reusable.Commander
{
    public interface ICommandServiceProvider
    {
        ILogger Logger { get; }

        ICommandLineExecutor Executor { get; }

        Identifier DefaultId { get; }
    }
    
    public class CommandServiceProvider<T> : ICommandServiceProvider where T : IConsoleCommand
    {
        public CommandServiceProvider(ILogger<T> logger, ICommandLineExecutor executor)
        {
            Logger = logger;
            Executor = executor;
        }

        public ILogger Logger { get; }

        public ICommandLineExecutor Executor { get; }

        public Identifier DefaultId => CommandHelper.GetCommandId(typeof(T));
    }
}