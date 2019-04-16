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
using Reusable.SmartConfig;

namespace Reusable.Commander
{
    public interface IConsoleCommand
    {
        [NotNull]
        Identifier Id { get; }

        Task ExecuteAsync([CanBeNull] object parameter, [CanBeNull] object context, CancellationToken cancellationToken = default);
    }

    [PublicAPI]
    public abstract class ConsoleCommand<TConfiguration, TContext> : IConsoleCommand where TConfiguration : ICommandConfiguration, new()
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
//                case null:
//                    await ExecuteWhenEnabledAsync(default, (TContext)context, cancellationToken);
//                    break;

                case ICommandLine commandLine:
                    await CheckAndExecuteAsync(Services.Mapper.Map<TConfiguration>(commandLine), (TContext)context, cancellationToken);
                    await CheckAndExecuteAsync
                    (
                        new Configuration<TConfiguration>(new CommandLineArgumentProvider(commandLine)),
                        (TContext)context,
                        cancellationToken
                    );
                    break;

//                case TBag bag:
//                    await ExecuteWhenEnabledAsync(bag, (TContext)context, cancellationToken);
//                    break;

                default:
                    throw new ArgumentOutOfRangeException
                    (
                        paramName: nameof(parameter),
                        message: $"{nameof(parameter)} must be either a {typeof(ICommandLine).Name} or {typeof(TConfiguration).Name}."
                    );
            }
        }

        private async Task CheckAndExecuteAsync(TConfiguration parameter, TContext context, CancellationToken cancellationToken)
        {
            if (await CanExecuteAsync(parameter, context, cancellationToken))
            {
                Logger.Log(Abstraction.Layer.Service().Decision("Execute command.").Because("Can execute."));
                await ExecuteAsync(parameter, context, cancellationToken);
            }
            else
            {
                Logger.Log(Abstraction.Layer.Service().Decision("Don't execute command.").Because("Cannot execute."));
            }
        }

        /// <summary>
        /// When overriden by a derived class indicates whether a command can be executed. The default implementation always returns 'true'.
        /// </summary>
        protected virtual Task<bool> CanExecuteAsync(TConfiguration parameter, TContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        protected abstract Task ExecuteAsync(TConfiguration parameter, TContext context, CancellationToken cancellationToken);

        // todo - new signature

        /// <summary>
        /// When overriden by a derived class indicates whether a command can be executed. The default implementation always returns 'true'.
        /// </summary>
        protected virtual Task<bool> CanExecuteAsync(IConfiguration<TConfiguration> configuration, TContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        protected virtual Task ExecuteAsync(IConfiguration<TConfiguration> configuration, TContext context, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task CheckAndExecuteAsync(IConfiguration<TConfiguration> configuration, TContext context, CancellationToken cancellationToken)
        {
            if (await CanExecuteAsync(configuration, context, cancellationToken))
            {
                Logger.Log(Abstraction.Layer.Service().Decision("Execute command.").Because("Can execute."));
                await ExecuteAsync(configuration, context, cancellationToken);
            }
            else
            {
                Logger.Log(Abstraction.Layer.Service().Decision("Don't execute command.").Because("Cannot execute."));
            }
        }
    }

    public abstract class SimpleCommand : ConsoleCommand<SimpleBag, NullContext>
    {
        protected SimpleCommand([NotNull] ICommandServiceProvider serviceProvider, Identifier id)
            : base(serviceProvider, id) { }
    }

    [UsedImplicitly]
    public class NullContext { }
}