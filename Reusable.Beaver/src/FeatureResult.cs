using System;
using JetBrains.Annotations;

namespace Reusable.Beaver
{
    public class FeatureResult<T> : IDisposable
    {
        public Feature Feature { get; set; } = default!;

        public FeatureState State { get; set; }

        [CanBeNull]
        public T Value { get; set; } = default!;

        public static implicit operator T(FeatureResult<T> result) => result.Value;

        public void Dispose() => (Value as IDisposable)?.Dispose();
    }
}