using Reusable.FeatureBuzz.Annotations;
using Reusable.FeatureBuzz.Policies.Abstractions;

namespace Reusable.FeatureBuzz.Policies
{
    /// <summary>
    /// Indicates that a feature is always enabled.
    /// </summary>
    [FeatureBuzz]
    public class Enabled : Flag
    {
        public Enabled() : base(true) { }
    }
}