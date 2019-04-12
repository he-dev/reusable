using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Attachments
{
    public class Timestamp<T> : LogAttachment where T : IDateTime, new()
    {
        private readonly IDateTime _dateTime;

        // There is no pretty way to get the name without `1

        public Timestamp() : base("Timestamp")
        {
            _dateTime = new T();
        }

        public override object Compute(ILog log)
        {
            return _dateTime.Now();
        }
    }
}