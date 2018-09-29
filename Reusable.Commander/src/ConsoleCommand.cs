using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.OmniLog;
using Reusable.Reflection;

namespace Reusable.Commander
{
    public interface IConsoleCommand
    {
        [NotNull]
        SoftKeySet Name { get; }

        Task ExecuteAsync([CanBeNull] object parameter, CancellationToken cancellationToken = default);
    }

    public interface ICommandServiceProvider
    {
        ILogger Logger { get; }

        ICommandLineMapper Mapper { get; }
        
        SoftKeySet Name { get; }
    }

    public class CommandServiceProvider<T> : ICommandServiceProvider where T : IConsoleCommand
    {
        public CommandServiceProvider(ILogger<T> logger, ICommandLineMapper mapper)
        {
            Logger = logger;
            Mapper = mapper;
        }

        public ILogger Logger { get; }

        public ICommandLineMapper Mapper { get; }

        public SoftKeySet Name => CommandHelper.GetCommandName(typeof(T));
    }

    public abstract class ConsoleCommand<TBag> : IConsoleCommand where TBag : ICommandBag, new()
    {
        private readonly ICommandServiceProvider _serviceProvider;

        protected ConsoleCommand([NotNull] ICommandServiceProvider serviceProvider, [CanBeNull] SoftKeySet name = default)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Name = name ?? serviceProvider.Name;
        }

        [NotNull]
        protected ILogger Logger => _serviceProvider.Logger;

        public SoftKeySet Name { get; }

        public async Task ExecuteAsync(object parameter, CancellationToken cancellationToken)
        {
            switch (parameter)
            {
                case null:
                    await ExecuteAsync(default, cancellationToken);
                    break;

                case ICommandLine commandLine:
                    await ExecuteAsync(_serviceProvider.Mapper.Map<TBag>(commandLine), cancellationToken);
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
        protected SimpleCommand([NotNull] ICommandServiceProvider serviceProvider, SoftKeySet name)
            : base(serviceProvider, name)
        {
        }
    }         
}