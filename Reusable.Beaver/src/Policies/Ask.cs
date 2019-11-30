using System;

namespace Reusable.Beaver.Policies
{
    public class Ask : IFeaturePolicy
    {
        private readonly Func<Feature, bool> _isEnabled;
        public Ask(string name, Func<Feature, bool> isEnabled) => (Feature, _isEnabled) = (name, isEnabled);
        public Feature Feature { get; }
        public bool IsEnabled(Feature feature) => _isEnabled(feature);
    }
}