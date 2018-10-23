using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;
using JetBrains.Annotations;
using Reusable.OmniLog;
using Reusable.Reflection;

namespace Reusable.Commander
{
    public interface IConsoleCommand
    {
        [NotNull]
        Identifier Id { get; }

        Task ExecuteAsync([CanBeNull] object parameter, CancellationToken cancellationToken = default);
    }

    [PublicAPI]
    public abstract class ConsoleCommand<TBag> : IConsoleCommand where TBag : ICommandBag, new()
    {
        private readonly ICommandServiceProvider _serviceProvider;

        protected ConsoleCommand([NotNull] ICommandServiceProvider serviceProvider, [CanBeNull] Identifier id = default)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Id = id ?? serviceProvider.DefaultId;
        }

        [NotNull]
        protected ILogger Logger => _serviceProvider.Logger;

        [NotNull]
        protected ICommandLineMapper Mapper => _serviceProvider.Mapper;
        
        [NotNull]
        protected ICommandLineExecutor Executor => _serviceProvider.Executor;

        public Identifier Id { get; }

        public async Task ExecuteAsync(object parameter, CancellationToken cancellationToken)
        {
            switch (parameter)
            {
                case null:
                    await ExecuteAsync(default, cancellationToken);
                    break;

                case ICommandLine commandLine:
                    await ExecuteAsync(Mapper.Map<TBag>(commandLine), cancellationToken);
                    break;

                case TBag bag:
                    await ExecuteAsync(bag, cancellationToken);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        paramName: nameof(parameter),
                        message: $"{nameof(parameter)} must be either a {typeof(ICommandLine).Name} or {typeof(TBag).Name}."
                    );
            }
        }

        protected abstract Task ExecuteAsync(TBag parameter, CancellationToken cancellationToken);
    }

    public abstract class SimpleCommand : ConsoleCommand<SimpleBag>
    {
        protected SimpleCommand([NotNull] ICommandServiceProvider serviceProvider, Identifier id)
            : base(serviceProvider, id)
        {
        }
    }
}