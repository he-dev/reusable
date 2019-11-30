using System;

namespace Reusable.Beaver
{
    public class FeatureActionResult<T> : IDisposable
    {
        public IFeaturePolicy Policy { get; internal set; }

        public T Value { get; internal set; }

        public static implicit operator T(FeatureActionResult<T> telemetry) => telemetry.Value;

        public void Dispose() => (Value as IDisposable)?.Dispose();

        public class Main : FeatureActionResult<T> { }

        public class Fallback : FeatureActionResult<T> { }
    }
}