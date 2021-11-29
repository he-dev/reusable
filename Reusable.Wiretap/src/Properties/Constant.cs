using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.Properties
{
    public class Constant : PropertyService
    {
        private readonly object value;

        public Constant(string name, object value) : base(name)
        {
            this.value = value;
        }

        public override object? GetValue(ILogEntry logEntry) => value;
    }
}