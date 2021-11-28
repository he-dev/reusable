using System;
using JetBrains.Annotations;

namespace Reusable.FeatureBuzz
{
    [PublicAPI]
    public record FeatureResult<T> : IDisposable
    {
        public Feature Feature { get; init; }

        [CanBeNull]
        public T Value { get; init; } = default!;

        public static implicit operator T(FeatureResult<T> result) => result.Value;

        public void Dispose()
        {
            (Value as IDisposable)?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}