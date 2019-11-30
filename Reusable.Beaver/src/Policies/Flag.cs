namespace Reusable.Beaver.Policies
{
    public abstract class Flag : IFeaturePolicy
    {
        protected Flag(string name, bool value) => (Feature, Value) = (name, value);
        public Feature Feature { get; }
        public bool Value { get; }
        public bool IsEnabled(Feature feature) => Value;
    }
}