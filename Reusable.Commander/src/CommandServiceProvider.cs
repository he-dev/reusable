using JetBrains.Annotations;
using Reusable.Commander.Services;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Commander
{
    public interface ICommandServiceProvider
    {
        [NotNull]
        ILogger Logger { get; }
        
        [NotNull]
        ICommandExecutor Executor { get; }

        [NotNull]
        Identifier CommandId { get; }
    }

    public class CommandServiceProvider<T> : ICommandServiceProvider where T : ICommand
    {
        public CommandServiceProvider(ILogger<T> logger, ICommandExecutor executor)
        {
            Logger = logger;
            Executor = executor;
            CommandId = CommandHelper.GetCommandId(typeof(T));
        }

        public ILogger Logger { get; }

        public ICommandExecutor Executor { get; }

        public Identifier CommandId { get; }
    }
}