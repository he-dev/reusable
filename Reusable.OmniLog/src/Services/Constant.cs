using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Services
{
    public class Constant : Service
    {
        private readonly object value;

        public Constant(string name, object value) : base(name)
        {
            this.value = value;
        }

        public override object? GetValue(LogEntry logEntry) => value;
    }
}