using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.Attachements
{
    public class Timestamp<T> : LogAttachement where T : IDateTime, new()
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