using System;
using JetBrains.Annotations;

namespace Reusable.Loggex.ComputedProperties
{
    internal class Lambda : ComputedProperty
    {
        private readonly Func<LogEntry, object> _compute;

        public Lambda(CaseInsensitiveString name, [NotNull] Func<LogEntry, object> compute) : base(name)
        {
            _compute = compute ?? throw new ArgumentNullException(nameof(compute));
        }

        public override object Compute(LogEntry logEntry) => _compute(logEntry);
    }
}