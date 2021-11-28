using JetBrains.Annotations;

namespace Reusable.FeatureBuzz
{
    [PublicAPI]
    public record FeatureContext
    {
        public IFeatureCollection Features { get; init; }

        public Feature Feature { get; init; }

        public object? Parameter { get; init; }
        
        public static FeatureContext Create(IFeatureCollection features, string name, object? parameter)
        {
            return new FeatureContext { Features = features, Feature = features[name], Parameter = parameter };
        }
    }
}