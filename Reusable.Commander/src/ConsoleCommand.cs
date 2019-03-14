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
using Reusable.Reflection;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.Commander
{
    public interface IConsoleCommand
    {
        [NotNull]
        Identifier Id { get; }

        //Task<bool> CanExecuteAsync([CanBeNull] object parameter, [CanBeNull] object context, CancellationToken cancellationToken = default);

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
                {
                    if (await CanExecuteAsync(default, (TContext)context, cancellationToken))
                    {
                        await ExecuteAsync(default, (TContext)context, cancellationToken);
                    }
                    else
                    {
                        LogCanceled();
                    }
                }
                    break;

                case ICommandLine commandLine:
                {
                    var bag = Services.Mapper.Map<TBag>(commandLine);
                    if (await CanExecuteAsync(bag, (TContext)context, cancellationToken))
                    {
                        await ExecuteAsync(bag, (TContext)context, cancellationToken);
                    }
                    else
                    {
                        LogCanceled();
                    }
                }
                    break;

                case TBag bag:
                {
                    if (await CanExecuteAsync(bag, (TContext)context, cancellationToken))
                    {
                        await ExecuteAsync(bag, (TContext)context, cancellationToken);
                    }
                    else
                    {
                        LogCanceled();
                    }
                }
                    break;

                default:
                {
                    throw new ArgumentOutOfRangeException
                    (
                        paramName: nameof(parameter),
                        message: $"{nameof(parameter)} must be either a {typeof(ICommandLine).Name} or {typeof(TBag).Name}."
                    );
                }
            }

            void LogCanceled() => Logger.Log(Abstraction.Layer.Infrastructure().Routine(nameof(ExecuteAsync)).Canceled(), $"{Id.ToString()} is disabled.");
        }

        protected virtual Task<bool> CanExecuteAsync(TBag parameter, TContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        protected abstract Task ExecuteAsync(TBag parameter, TContext context, CancellationToken cancellationToken);
    }

    public abstract class SimpleCommand : ConsoleCommand<SimpleBag, object>
    {
        protected SimpleCommand([NotNull] ICommandServiceProvider serviceProvider, Identifier id)
            : base(serviceProvider, id) { }
    }
}