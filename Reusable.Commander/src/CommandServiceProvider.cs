using Reusable.OmniLog;

namespace Reusable.Commander
{
    public interface ICommandServiceProvider
    {
        ILogger Logger { get; }

        ICommandLineMapper Mapper { get; }

        ICommandLineExecutor Executor { get; }

        Identifier DefaultId { get; }
    }
    
    public class CommandServiceProvider<T> : ICommandServiceProvider where T : IConsoleCommand
    {
        public CommandServiceProvider(ILogger<T> logger, ICommandLineExecutor executor, ICommandLineMapper mapper)
        {
            Logger = logger;
            Executor = executor;
            Mapper = mapper;
        }

        public ILogger Logger { get; }
        
        public ICommandLineMapper Mapper { get; }

        public ICommandLineExecutor Executor { get; }

        public Identifier DefaultId => CommandHelper.GetCommandId(typeof(T));
    }
}