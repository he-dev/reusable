using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    public readonly struct ResourceMetadataScope<T>
    {
        public ResourceMetadataScope([CanBeNull] ResourceMetadata metadata) => Metadata = metadata ?? ResourceMetadata.Empty;

        public ResourceMetadata Metadata { get; }

        public static implicit operator ResourceMetadataScope<T>(ResourceMetadata metadata) => new ResourceMetadataScope<T>(metadata);

        public static implicit operator ResourceMetadata(ResourceMetadataScope<T> scope) => scope.Metadata;
    }
}