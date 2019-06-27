using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

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
            [NotNull] CommandServiceProvider<Lambda<TCommandLine, TContext>> serviceProvider,
            [NotNull] Identifier id,
            [NotNull] ExecuteCallback<TCommandLine, TContext> execute
        )
            : base(serviceProvider, id)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        protected override Task ExecuteAsync(TCommandLine commandLine, TContext context, CancellationToken cancellationToken)
        {
            return _execute(Id, commandLine, context, cancellationToken);
        }
    }
}