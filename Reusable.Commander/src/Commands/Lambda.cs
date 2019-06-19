using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Commander.Services;

namespace Reusable.Commander.Commands
{
    public delegate Task ExecuteCallback<T, in TContext>
    (
        Identifier commandId,
        ICommandLineReader<T> parameters,
        TContext context,
        CancellationToken cancellationToken = default
    ) where T : ICommandParameter;

    public class Lambda<TParameter, TContext> : Command<TParameter, TContext> where TParameter : ICommandParameter
    {
        private readonly ExecuteCallback<TParameter, TContext> _execute;

        public delegate Lambda<T, TContext> Factory<T>([NotNull] Identifier id, [NotNull] ExecuteCallback<TParameter, TContext> execute) where T : ICommandParameter, new();

        public Lambda
        (
            [NotNull] CommandServiceProvider<Lambda<TParameter, TContext>> serviceProvider,
            [NotNull] Identifier id,
            [NotNull] ExecuteCallback<TParameter, TContext> execute
        )
            : base(serviceProvider, id)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        protected override Task ExecuteAsync(ICommandLineReader<TParameter> parameter, TContext context, CancellationToken cancellationToken)
        {
            return _execute(Id, parameter, context, cancellationToken);
        }
        
    }
}