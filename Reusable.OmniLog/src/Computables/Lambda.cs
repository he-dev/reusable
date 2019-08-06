using System;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Computables
{
    public class Lambda : Computable
    {
        private readonly Func<LogEntry, object> _compute;

        public Lambda([NotNull] string name, [NotNull] Func<LogEntry, object> compute) : base(name)
        {
            _compute = compute ?? throw new ArgumentNullException(nameof(compute));
        }

        public override object Compute(LogEntry logEntry) => _compute(logEntry);
    }
}