using System;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Scalars
{
    public class Lambda : Computable
    {
        private readonly Func<LogEntry, object> _compute;

        public Lambda(string name, Func<LogEntry, object> compute) : base(name)
        {
            _compute = compute;
        }

        public override object? Compute(LogEntry logEntry) => _compute(logEntry);
    }
}