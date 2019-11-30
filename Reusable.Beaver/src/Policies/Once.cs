namespace Reusable.Beaver.Policies
{
    public class Once : IFeaturePolicy, IFinalizable
    {
        public Once(string name) => Feature = name;
        public Feature Feature { get; }
        public bool IsEnabled(Feature feature) => true;
        public void FinallyMain(Feature feature) => feature.Toggle?.Remove(Feature);
        public void FinallyFallback(Feature feature) { }
        public void FinallyIIf(Feature feature) { }
    }
}