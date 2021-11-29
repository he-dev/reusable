using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Properties
{
    public class Timestamp<T> : PropertyService where T : IDateTime, new()
    {
        private readonly IDateTime _dateTime;

        // There is no pretty way to get the name without `1
        public Timestamp() : base(nameof(Timestamp))
        {
            _dateTime = new T();
        }

        public override object? GetValue(ILogEntry logEntry)
        {
            return _dateTime.Now();
        }
    }
}