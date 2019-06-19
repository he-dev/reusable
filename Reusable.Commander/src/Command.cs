using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.Commander.Services;

namespace Reusable.Commander
{
    public interface ICommand
    {
        [NotNull]
        Identifier Id { get; }

        Task ExecuteAsync([CanBeNull] object parameter, [CanBeNull] object context, CancellationToken cancellationToken = default);
    }

    [PublicAPI]
    public abstract class Command<TParameter, TContext> : ICommand where TParameter : ICommandParameter //, new()
    {
        protected Command([NotNull] ICommandServiceProvider serviceProvider, [CanBeNull] Identifier id = default)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Id = id ?? serviceProvider.CommandId;
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
                    await CheckAndExecuteAsync(default, (TContext)context, cancellationToken);
                    break;

                case ICommandLine commandLine:
                    await CheckAndExecuteAsync(new CommandLineReader<TParameter>(commandLine), (TContext)context, cancellationToken);
                    break;

                default:
                    throw new ArgumentOutOfRangeException
                    (
                        paramName: nameof(parameter),
                        message: $"{nameof(parameter)} must be either a {typeof(ICommandLine).Name} or {typeof(TParameter).Name}."
                    );
            }
        }

        private async Task CheckAndExecuteAsync(ICommandLineReader<TParameter> parameter, TContext context, CancellationToken cancellationToken)
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
        protected virtual Task<bool> CanExecuteAsync(ICommandLineReader<TParameter> parameter, TContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        protected abstract Task ExecuteAsync(ICommandLineReader<TParameter> parameter, TContext context, CancellationToken cancellationToken);
    }

    public abstract class Command<TParameter> : Command<TParameter, NullContext> where TParameter : ICommandParameter
    {
        protected Command([NotNull] ICommandServiceProvider serviceProvider, [CanBeNull] Identifier id = default) : base(serviceProvider, id) { }
    }

    public abstract class Command : Command<ICommandParameter, NullContext>
    {
        protected Command([NotNull] ICommandServiceProvider serviceProvider, Identifier id)
            : base(serviceProvider, id) { }
    }

    [UsedImplicitly]
    public class NullContext
    {
        public static readonly NullContext Default = new NullContext();
    }
}