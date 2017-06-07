using System;

namespace Reusable.Logging.ComputedProperties
{
    public class Now : ComputedProperty
    {
        public override object Compute(LogEntry log) => DateTime.Now;
    }
}