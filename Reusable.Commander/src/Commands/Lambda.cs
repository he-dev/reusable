using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.OmniLog;

namespace Reusable.Commander.Commands
{
    public delegate Task ExecuteCallback<in T>(SoftKeySet commandName, T bag, CancellationToken cancellationToken = default) where T : ICommandBag, new();

    public class Lambda<TBag> : ConsoleCommand<TBag> where TBag : ICommandBag, new()
    {
        private readonly ExecuteCallback<TBag> _execute;

        public Lambda(CommandServiceProvider<Lambda<TBag>> serviceProvider, SoftKeySet name, [NotNull] ExecuteCallback<TBag> execute)
            : base(serviceProvider, name)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        protected override  async Task ExecuteAsync(TBag parameter, CancellationToken cancellationToken) => await _execute(Name, parameter, cancellationToken);
    }
}