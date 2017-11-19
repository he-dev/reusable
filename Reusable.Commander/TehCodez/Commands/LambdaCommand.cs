using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.CommandLine;
using Reusable.OmniLog;

namespace Reusable.Commander.Commands
{
    public class LambdaCommand<TLocal> : ConsoleCommand
    {
        private readonly TLocal _local;
        
        private readonly Func<TLocal, CancellationToken, Task> _execute;

        public LambdaCommand(ILoggerFactory loggerFactory, TLocal local, [NotNull] Func<TLocal, CancellationToken, Task> execute) : base(loggerFactory)
        {
            _local = local;
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            return _execute(_local, cancellationToken);
        }
    }
}