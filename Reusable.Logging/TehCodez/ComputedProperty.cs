namespace Reusable.Logging.Loggex
{
    public interface IComputedProperty
    {
        CaseInsensitiveString Name { get; }

        object Compute(LogEntry log);
    }

    public abstract class ComputedProperty : IComputedProperty
    {
        protected ComputedProperty(CaseInsensitiveString name) => Name = name;

        protected ComputedProperty() => Name = GetType().Name;

        public CaseInsensitiveString Name { get; }

        public abstract object Compute(LogEntry log);
    }
}
