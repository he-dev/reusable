using System;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Services
{
    public class Lambda : Service
    {
        private readonly Func<LogEntry, object> getValue;

        public Lambda(string name, Func<LogEntry, object> getValue) : base(name)
        {
            this.getValue = getValue;
        }

        public override object? GetValue(LogEntry logEntry) => getValue(logEntry);
    }
}