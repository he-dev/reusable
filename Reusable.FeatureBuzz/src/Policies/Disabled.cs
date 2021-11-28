using Reusable.FeatureBuzz.Policies.Abstractions;

namespace Reusable.FeatureBuzz.Policies
{
    /// <summary>
    /// Indicates that a feature is always disabled.
    /// </summary>
    public class Disabled : Flag
    {
        public Disabled() : base(false) { }
    }
}