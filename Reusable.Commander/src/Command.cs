using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        protected Command([NotNull] ILogger logger)
        {
            Logger = logger;
            Id = CommandHelper.GetCommandId(GetType());
        }

        // You set this to "protected internal" aka (public) so that the DecoratorCommand can access it when calling "base".
        [NotNull]
        protected internal ILogger Logger { get; }

        public virtual Identifier Id { get; }
        
        [DebuggerStepThrough]
        public virtual async Task ExecuteAsync(object argument, object context, CancellationToken cancellationToken)
        {
            var commandLine = CommandLine.Create<TCommandLine>(argument);

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
        protected Command(ILogger logger) : base(logger) { }
    }

    public abstract class SimpleCommand : Command<ICommandLine, object>
    {
        protected SimpleCommand(ILogger logger) : base(logger) { }
    }

    [UsedImplicitly]
    public abstract class DecoratorCommand : ICommand
    {
        protected DecoratorCommand(ICommand command) { Command = command; }

        protected ICommand Command { get; }

        public virtual Identifier Id => Command.Id;

        public abstract Task ExecuteAsync(object argument, object context, CancellationToken cancellationToken);
    }
}