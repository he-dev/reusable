namespace Reusable.Beaver.Policies
{
    /// <summary>
    /// Indicates that a feature is always enabled.
    /// </summary>
    [FeatureToggle]
    public class AlwaysOn : Flag
    {
        public AlwaysOn() : base(true) { }
    }
}