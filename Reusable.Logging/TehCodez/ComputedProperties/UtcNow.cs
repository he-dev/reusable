using System;

namespace Reusable.Loggex.ComputedProperties
{
    public class UtcNow : ComputedProperty
    {
        public override object Compute(LogEntry log) => DateTime.UtcNow;
    }
}