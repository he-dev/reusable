using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Beaver
{
    public class FeatureToggleAttribute : NamespaceAttribute
    {
        public FeatureToggleAttribute() : base(nameof(FeatureToggle))
        {
            Alias = "FT";
        }
    }
}