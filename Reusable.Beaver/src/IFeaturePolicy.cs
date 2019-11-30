namespace Reusable.Beaver
{
    public interface IFeaturePolicy
    {
        Feature Feature { get; }
        bool IsEnabled(Feature feature);
    }
}