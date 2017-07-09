using System;

namespace Reusable.Loggex.ComputedProperties
{
    public class Now : ComputedProperty
    {
        public override object Compute(LogEntry log) => DateTime.Now;
    }
}