using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.OmniLog;

namespace Reusable.Commander.Commands
{
    public class LambdaCommand<TBag> : ConsoleCommand<TBag> where TBag : new()
    {
        private readonly Func<TBag, CancellationToken, Task> _execute;

        public delegate LambdaCommand<TBag> Factory(SoftKeySet name, Func<TBag, CancellationToken, Task> execute);

        public LambdaCommand(ILogger<LambdaCommand<TBag>> logger, ICommandLineMapper mapper, SoftKeySet name, [NotNull] Func<TBag, CancellationToken, Task> execute) 
            : base(logger, mapper)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        protected override Task ExecuteAsync(TBag parameter, CancellationToken cancellationToken)
        {
            return _execute(parameter, cancellationToken);
        }
    }
}