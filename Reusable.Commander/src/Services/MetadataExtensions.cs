using Reusable.IOnymous;

namespace Reusable.Commander.Services
{
    internal static class MetadataExtensions
    {
        public static MetadataScope<ICommandLine> CommandLine(this Metadata metadata)
        {
            return metadata.For<ICommandLine>();
        }

        public static Metadata CommandLine(this Metadata metadata, ConfigureMetadataScopeCallback<ICommandLine> scope)
        {
            return metadata.For(scope);
        }

// ---
        public static object DefaultValue(this MetadataScope<ICommandLine> scope)
        {
            return scope.Metadata.GetItemByCallerName(SoftString.Empty);
        }

        public static MetadataScope<ICommandLine> DefaultValue(this MetadataScope<ICommandLine> scope, object name)
        {
            return scope.Metadata.SetItemByCallerName(name);
        }
    }
}