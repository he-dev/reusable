using System;

namespace Reusable.Loggex.ComputedProperties
{
    internal class Lambda : IComputedProperty
    {
        private readonly Func<LogEntry, object> _compute;

        public Lambda(CaseInsensitiveString name, Func<LogEntry, object> compute)
        {
            Name = name;
            _compute = compute;
        }

        public CaseInsensitiveString Name { get; }

        public object Compute(LogEntry logEntry) => _compute(logEntry);
    }
}