using System;

namespace Reusable.Logging.ComputedProperties
{
    internal class Lambda : IComputedProperty
    {
        private readonly Func<LogEntry, object> _compute;

        public Lambda(string name, Func<LogEntry, object> compute)
        {
            Name = name;
            _compute = compute;
        }

        public string Name { get; }

        public object Compute(LogEntry logEntry) => _compute(logEntry);
    }
}