using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Beaver.Annotations
{
    public class BeaverAttribute : NamespaceAttribute
    {
        public BeaverAttribute() : base(nameof(FeatureToggle))
        {
            Alias = "B";
        }
    }
}