using System;

namespace Reusable.Logging.Loggex.ComputedProperties
{
    public class UtcNow : ComputedProperty
    {
        public override object Compute(LogEntry log) => DateTime.UtcNow;
    }
}