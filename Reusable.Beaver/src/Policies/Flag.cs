namespace Reusable.Beaver.Policies
{
    public abstract class Flag : FeaturePolicy
    {
        private readonly bool _value;
        
        protected Flag(bool enabled) => _value = enabled;

        public override FeatureState State(FeatureContext context) => _value ? FeatureState.Enabled : FeatureState.Disabled;
    }
}