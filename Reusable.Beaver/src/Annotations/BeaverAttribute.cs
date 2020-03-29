using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Beaver.Annotations
{
    public class BeaverAttribute : JsonTypeSchemaAttribute
    {
        public BeaverAttribute() : base(nameof(FeatureToggle))
        {
            Alias = "B";
        }
    }
}