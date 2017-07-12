namespace Reusable.Logging.Loggex.Tests.Mocks
{
    class MockProperty : ComputedProperty
    {
        private readonly object _value;
        public MockProperty(object value) => _value = value;
        public override object Compute(LogEntry log) => _value;
    }
}
