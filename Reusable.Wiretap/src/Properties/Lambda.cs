using System;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Properties
{
    public class Lambda : PropertyService
    {
        private readonly Func<ILogEntry, object> getValue;

        public Lambda(string name, Func<ILogEntry, object> getValue) : base(name)
        {
            this.getValue = getValue;
        }

        public override object? GetValue(ILogEntry logEntry) => getValue(logEntry);
    }
}