using JetBrains.Annotations;
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
        ICommandLineExecutor Executor { get; }

        [NotNull]
        Identifier Id { get; }
    }

    public class CommandServiceProvider<T> : ICommandServiceProvider where T : IConsoleCommand
    {
        public CommandServiceProvider(ILogger<T> logger, ICommandLineExecutor executor, ICommandLineMapper mapper)
        {
            Logger = logger;
            Executor = executor;
            Mapper = mapper;
            Id = CommandHelper.GetCommandId(typeof(T));
        }

        public ILogger Logger { get; }

        public ICommandLineMapper Mapper { get; }

        public ICommandLineExecutor Executor { get; }

        public Identifier Id { get; }
    }
}