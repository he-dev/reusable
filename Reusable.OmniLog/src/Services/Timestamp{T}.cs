using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

namespace Reusable.OmniLog.Services
{
    public class Timestamp<T> : Service where T : IDateTime, new()
    {
        private readonly IDateTime _dateTime;

        // There is no pretty way to get the name without `1
        public Timestamp() : base("Timestamp")
        {
            _dateTime = new T();
        }

        public override object? GetValue(LogEntry logEntry)
        {
            return _dateTime.Now();
        }
    }
}