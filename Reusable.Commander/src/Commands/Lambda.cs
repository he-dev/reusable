using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Commander.Commands
{
    public delegate Task ExecuteCallback<in TCommandLine, in TContext>
    (
        Identifier commandId,
        TCommandLine parameters,
        TContext context,
        CancellationToken cancellationToken = default
    ) where TCommandLine : ICommandLine;

    public class Lambda<TCommandLine, TContext> : Command<TCommandLine, TContext> where TCommandLine : class, ICommandLine
    {
        private readonly ExecuteCallback<TCommandLine, TContext> _execute;

        public delegate Lambda<T, TContext> Factory<T>([NotNull] Identifier id, [NotNull] ExecuteCallback<TCommandLine, TContext> execute) where T : class, ICommandLine, new();

        public Lambda
        (
            ILogger<Lambda<TCommandLine, TContext>> logger,
            [NotNull] Identifier id,
            [NotNull] ExecuteCallback<TCommandLine, TContext> execute
        )
            : base(logger)
        {
            Id = id;
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        [NotNull]
        public override Identifier Id { get; }

        protected override Task ExecuteAsync(TCommandLine commandLine, TContext context, CancellationToken cancellationToken)
        {
            return _execute(Id, commandLine, context, cancellationToken);
        }
    }
}