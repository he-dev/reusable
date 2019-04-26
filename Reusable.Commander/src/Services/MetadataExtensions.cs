using Reusable.IOnymous;

namespace Reusable.Commander.Services
{
    internal static class MetadataExtensions
    {
        public static Metadata<ICommandLine> CommandLine(this Metadata metadata)
        {
            return metadata.Scope<ICommandLine>();
        }

        public static Metadata CommandLine(this Metadata metadata, ConfigureMetadataScopeCallback<ICommandLine> scope)
        {
            return metadata.Scope(scope);
        }

// ---
        public static object DefaultValue(this Metadata<ICommandLine> scope)
        {
            return scope.Value.GetItemByCallerName(SoftString.Empty);
        }

        public static Metadata<ICommandLine> DefaultValue(this Metadata<ICommandLine> scope, object name)
        {
            return scope.Value.SetItemByCallerName(name);
        }
    }
}