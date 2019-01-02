using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public readonly struct ResourceMetadataScope<T>
    {
        public ResourceMetadataScope(ResourceMetadata metadata) => Metadata = metadata;

        public ResourceMetadata Metadata { get; }

        public static implicit operator ResourceMetadataScope<T>(ResourceMetadata metadata) => new ResourceMetadataScope<T>(metadata);

        public static implicit operator ResourceMetadata(ResourceMetadataScope<T> scope) => scope.Metadata;
    }
}