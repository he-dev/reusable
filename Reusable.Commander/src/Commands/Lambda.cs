using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.OmniLog;

namespace Reusable.Commander.Commands
{
    public delegate Task ExecuteCallback<in T>(Identifier commandId, T bag, CancellationToken cancellationToken = default) where T : ICommandParameter, new();

    public class Lambda<TBag> : ConsoleCommand<TBag, object> where TBag : ICommandParameter, new()
    {
        private readonly ExecuteCallback<TBag> _execute;

        public delegate Lambda<T> Factory<T>([NotNull] Identifier id, [NotNull] ExecuteCallback<TBag> execute) where T : ICommandParameter, new();

        public Lambda(CommandServiceProvider<Lambda<TBag>> serviceProvider, [NotNull] Identifier id, [NotNull] ExecuteCallback<TBag> execute)
            : base(serviceProvider, id)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        protected override  async Task ExecuteAsync(TBag parameter, object context, CancellationToken cancellationToken) => await _execute(Id, parameter, cancellationToken);
    }
}