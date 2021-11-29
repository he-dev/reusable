using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.FeatureBuzz.Annotations
{
    public class FeatureBuzzAttribute : JsonTypeSchemaAttribute
    {
        public FeatureBuzzAttribute() : base(nameof(FeatureCollection))
        {
            Alias = "B";
        }
    }
}