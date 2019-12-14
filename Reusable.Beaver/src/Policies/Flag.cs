namespace Reusable.Beaver.Policies
{
    public abstract class Flag : IFeaturePolicy
    {
        private readonly bool _value;
        
        protected Flag(bool enabled) => _value = enabled;

        public FeatureState State(FeatureContext context) => _value ? FeatureState.Enabled : FeatureState.Disabled;
    }
}