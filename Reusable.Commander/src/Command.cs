using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.Commander
{
    public interface ICommand
    {
        [NotNull]
        NameSet Name { get; }

        Task ExecuteAsync([CanBeNull] object argument, [CanBeNull] object context, CancellationToken cancellationToken = default);
    }

    [PublicAPI]
    public abstract class Command<TCommandLine, TContext> : ICommand where TCommandLine : class, ICommandLine
    {
        protected Command([NotNull] ILogger logger)
        {
            Logger = logger;
            Name = CommandHelper.GetCommandId(GetType());
        }

        [NotNull]
        protected ILogger Logger { get; }

        public virtual NameSet Name { get; }

        public virtual async Task ExecuteAsync(object argument, object context, CancellationToken cancellationToken)
        {
            var commandLine = CommandLine.Create<TCommandLine>(argument);
            await ExecuteAsync(commandLine, (TContext)context, cancellationToken);
        }

        protected abstract Task ExecuteAsync(TCommandLine commandLine, TContext context, CancellationToken cancellationToken);
    }

    public abstract class Command<TCommandLine> : Command<TCommandLine, object> where TCommandLine : class, ICommandLine
    {
        protected Command(ILogger logger) : base(logger) { }
    }

//    public abstract class SimpleCommand : Command<ICommandLine, object>
//    {
//        protected SimpleCommand(ILogger logger) : base(logger) { }
//    }

    [UsedImplicitly]
    [PublicAPI]
    public abstract class Decorator : ICommand
    {
        protected Decorator([NotNull] ICommand command)
        {
            Command = command ?? throw new ArgumentNullException(nameof(command));
        }

        [NotNull]
        protected ICommand Command { get; }

        public virtual NameSet Name => Command.Name;

        public abstract Task ExecuteAsync(object argument, object context, CancellationToken cancellationToken);
    }

    public class Logger : Decorator
    {
        private readonly ILogger<Logger> _logger;

        public Logger([NotNull] ICommand command, ILogger<Logger> logger) : base(command)
        {
            _logger = logger;
        }

        public override async Task ExecuteAsync(object argument, object context, CancellationToken cancellationToken)
        {
            using (_logger.UseScope(correlationHandle: "Command"))
            using (_logger.UseStopwatch())
            {
                _logger.Log(Abstraction.Layer.Service().Meta(new { CommandName = Command.Name.Default.ToString() }).Trace());
                try
                {
                    await Command.ExecuteAsync(argument, context, cancellationToken);
                    _logger.Log(Abstraction.Layer.Service().Routine(nameof(ICommand.ExecuteAsync)).Completed());
                }
                catch (OperationCanceledException)
                {
                    _logger.Log(Abstraction.Layer.Service().Routine(nameof(ICommand.ExecuteAsync)).Canceled(), "Cancelled.");
                    throw;
                }
                catch (Exception taskEx)
                {
                    _logger.Log(Abstraction.Layer.Service().Routine(nameof(ICommand.ExecuteAsync)).Faulted(), taskEx);
                    throw;
                }
            }
        }
    }
}