using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;

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

        public override object Compute(Log log)
        {
            return _dateTime.Now();
        }
    }
}