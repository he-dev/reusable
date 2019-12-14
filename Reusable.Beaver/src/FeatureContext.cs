namespace Reusable.Beaver
{
    public class FeatureContext
    {
        public FeatureContext(IFeatureToggle toggle, Feature feature, object? parameter)
        {
            Toggle = toggle;
            Feature = feature;
            Parameter = parameter;
        }

        public IFeatureToggle Toggle { get; }

        public Feature Feature { get; }

        public object? Parameter { get; }
    }
}