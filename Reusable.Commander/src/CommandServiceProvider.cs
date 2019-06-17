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
        ICommandLineMapper Mapper { get; }

        [NotNull]
        ICommandExecutor Executor { get; }

        [NotNull]
        Identifier CommandId { get; }
    }

    public class CommandServiceProvider<T> : ICommandServiceProvider where T : ICommand
    {
        public CommandServiceProvider(ILogger<T> logger, ICommandExecutor executor, ICommandLineMapper mapper)
        {
            Logger = logger;
            Executor = executor;
            Mapper = mapper;
            CommandId = CommandHelper.GetCommandId(typeof(T));
        }

        public ILogger Logger { get; }

        public ICommandLineMapper Mapper { get; }

        public ICommandExecutor Executor { get; }

        public Identifier CommandId { get; }
    }
}