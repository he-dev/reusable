namespace Reusable.IOnymous
{
    public static class ResourceMetadataKeys
    {
        public static string ProviderDefaultName { get; } = nameof(ProviderDefaultName);
        public static string ProviderCustomName { get; } = nameof(ProviderCustomName);
        public static string CanGet { get; } = nameof(CanGet);
        public static string CanPost { get; } = nameof(CanPost);
        public static string CanPut { get; } = nameof(CanPut);
        public static string CanDelete { get; } = nameof(CanDelete);
        public static string Scheme { get; } = nameof(Scheme);
        public static string Serializer { get; } = nameof(Serializer);
    }
}