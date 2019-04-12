using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;
using JetBrains.Annotations;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.Reflection;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.Commander
{
    public interface IConsoleCommand
    {
        [NotNull]
        Identifier Id { get; }

        Task ExecuteAsync([CanBeNull] object parameter, [CanBeNull] object context, CancellationToken cancellationToken = default);
    }

    [PublicAPI]
    public abstract class ConsoleCommand<TBag, TContext> : IConsoleCommand where TBag : ICommandBag, new()
    {
        protected ConsoleCommand([NotNull] ICommandServiceProvider serviceProvider, [CanBeNull] Identifier id = default)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Id = id ?? serviceProvider.Id;
        }

        [NotNull]
        protected ICommandServiceProvider Services { get; }

        [NotNull]
        protected ILogger Logger => Services.Logger;

        public Identifier Id { get; }

        public virtual async Task ExecuteAsync(object parameter, object context, CancellationToken cancellationToken)
        {
            switch (parameter)
            {
                case null:
                    await ExecuteWhenEnabledAsync(default, (TContext)context, cancellationToken);
                    break;

                case ICommandLine commandLine:
                    await ExecuteWhenEnabledAsync(Services.Mapper.Map<TBag>(commandLine), (TContext)context, cancellationToken);
                    break;

                case TBag bag:
                    await ExecuteWhenEnabledAsync(bag, (TContext)context, cancellationToken);
                    break;

                default:
                    throw new ArgumentOutOfRangeException
                    (
                        paramName: nameof(parameter),
                        message: $"{nameof(parameter)} must be either a {typeof(ICommandLine).Name} or {typeof(TBag).Name}."
                    );
            }
        }

        private async Task ExecuteWhenEnabledAsync(TBag parameter, TContext context, CancellationToken cancellationToken)
        {
            if (await CanExecuteAsync(parameter, context, cancellationToken))
            {
                await ExecuteAsync(parameter, context, cancellationToken);
            }
            else
            {
                Logger.Log(Abstraction.Layer.Infrastructure().Routine(nameof(ExecuteAsync)).Canceled(), $"'{Id}' is disabled.");
            }
        }

        /// <summary>
        /// When overriden by a derived class indicates whether a command can be executed. The default implementation always returns 'true'.
        /// </summary>
        protected virtual Task<bool> CanExecuteAsync(TBag parameter, TContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        protected abstract Task ExecuteAsync(TBag parameter, TContext context, CancellationToken cancellationToken);
    }

    public abstract class SimpleCommand : ConsoleCommand<SimpleBag, NullContext>
    {
        protected SimpleCommand([NotNull] ICommandServiceProvider serviceProvider, Identifier id)
            : base(serviceProvider, id) { }
    }

    [UsedImplicitly]
    public class NullContext { }
}