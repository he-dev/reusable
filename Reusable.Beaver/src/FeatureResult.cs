using System;

namespace Reusable.Beaver
{
    public class FeatureResult<T> : IDisposable
    {
        public T Value { get; set; }

        public Feature Feature { get; set; }

        public FeatureState State { get; set; }

        public static implicit operator T(FeatureResult<T> result) => result.Value;

        public void Dispose() => (Value as IDisposable)?.Dispose();
    }
}