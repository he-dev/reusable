namespace Reusable.Beaver
{
    public class FeatureActionResult<T>
    {
        public IFeaturePolicy Policy { get; set; }
        public T Value { get; set; }
        public override string ToString() => GetType().Name;
        public static implicit operator T(FeatureActionResult<T> telemetry) => telemetry.Value;

        public class Main : FeatureActionResult<T> { }

        public class Fallback : FeatureActionResult<T> { }
    }
}