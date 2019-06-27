using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.Commander
{
    public interface ICommand
    {
        [NotNull]
        Identifier Id { get; }

        Task ExecuteAsync([CanBeNull] object argument, [CanBeNull] object context, CancellationToken cancellationToken = default);
    }

    [PublicAPI]
    public abstract class Command<TCommandLine, TContext> : ICommand where TCommandLine : class, ICommandLine
    {
        [NotNull] private readonly ConstructorInfo _commandLineCtor;

        protected Command([NotNull] ICommandServiceProvider serviceProvider, [CanBeNull] Identifier id = default)
        {
            Services = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            Id = id ?? serviceProvider.CommandId;
            _commandLineCtor =
                typeof(TCommandLine).GetConstructor(new[] { typeof(CommandLineDictionary) }) ??
                throw new ArgumentException($"{typeof(TCommandLine).ToPrettyString()} must have the following constructor: ctor{nameof(CommandLineDictionary)}");
        }

        [NotNull]
        protected ICommandServiceProvider Services { get; }

        [NotNull]
        protected ILogger Logger => Services.Logger;

        public Identifier Id { get; }

        public virtual async Task ExecuteAsync(object argument, object context, CancellationToken cancellationToken)
        {
            var commandLine = CreateCommandLine(argument);

            if (await CanExecuteAsync(commandLine, (TContext)context, cancellationToken))
            {
                Logger.Log(Abstraction.Layer.Service().Decision("Execute command.").Because("Can execute."));
                await ExecuteAsync(commandLine, (TContext)context, cancellationToken);
            }
            else
            {
                Logger.Log(Abstraction.Layer.Service().Decision("Don't execute command.").Because("Cannot execute."));
            }
        }

        private TCommandLine CreateCommandLine(object argument)
        {
            if (argument is TCommandLine commandLine)
            {
                return commandLine;
            }

            return
                argument is CommandLineDictionary arguments
                    ? (TCommandLine)_commandLineCtor.Invoke(new object[] { arguments })
                    : default;
        }

        /// <summary>
        /// When overriden by a derived class indicates whether a command can be executed. The default implementation always returns 'true'.
        /// </summary>
        protected virtual Task<bool> CanExecuteAsync(TCommandLine parameter, TContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        protected abstract Task ExecuteAsync(TCommandLine commandLine, TContext context, CancellationToken cancellationToken);
    }

    public abstract class Command<TCommandLine> : Command<TCommandLine, object> where TCommandLine : class, ICommandLine
    {
        protected Command([NotNull] ICommandServiceProvider serviceProvider, [CanBeNull] Identifier id = default) : base(serviceProvider, id) { }
    }

    public abstract class Command : Command<ICommandLine, object>
    {
        protected Command([NotNull] ICommandServiceProvider serviceProvider, Identifier id)
            : base(serviceProvider, id) { }
    }
}