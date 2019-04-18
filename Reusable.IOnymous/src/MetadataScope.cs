using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    /// <summary>
    /// Provides a level of abstraction for metadata by adding the scope by T so that extensions can be grouped.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [PublicAPI]
    public readonly struct MetadataScope<T>
    {
        public MetadataScope(Metadata metadata) => Metadata = metadata;

        public Metadata Metadata { get; }

        public static implicit operator MetadataScope<T>(Metadata metadata) => new MetadataScope<T>(metadata);

        public static implicit operator Metadata(MetadataScope<T> scope) => scope.Metadata;
    }
}