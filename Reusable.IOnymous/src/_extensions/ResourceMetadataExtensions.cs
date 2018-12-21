using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    public static class ResourceMetadataExtensions
    {
        public static bool TryGetValue<T>([CanBeNull] this ResourceMetadata metadata, [NotNull] SoftString key, [CanBeNull] out T value)
        {
            value = default;

            if (metadata is null)
            {
                return false;
            }

            if (metadata.TryGetValue(key, out var x) && x is T result)
            {
                value = result;
                return true;
            }

            return false;
        }

        public static T GetValueOrDefault<T>([CanBeNull] this ResourceMetadata metadata, [NotNull] SoftString key)
        {
            return 
                metadata.TryGetValue(key, out T value) 
                    ? value 
                    : default;
        }

        public static string ProviderDefaultName(this ResourceMetadata metadata)
        {
            return
                metadata
                    .TryGetValue(nameof(ProviderDefaultName), out string name)
                    ? name
                    : string.Empty;
        }

        public static string ProviderCustomName(this ResourceMetadata metadata)
        {
            return
                metadata
                    .TryGetValue(nameof(ProviderCustomName), out string name)
                    ? name
                    : string.Empty;
        }

        public static IEnumerable<string> ProviderNames(this ResourceMetadata metadata)
        {
            if (metadata.TryGetValue(ResourceMetadataKeys.ProviderCustomName, out string customName))
            {
                yield return customName;
            }

            if (metadata.TryGetValue(ResourceMetadataKeys.ProviderDefaultName, out string defaultName))
            {
                yield return defaultName;
            }
        }

        public static IImmutableSet<SoftString> SchemeSet(this ResourceMetadata metadata) => metadata.GetValueOrDefault<IImmutableSet<SoftString>>(ResourceMetadataKeys.SchemeSet);

        public static ResourceMetadata AddScheme(this ResourceMetadata metadata, string scheme)
        {
            if (metadata.TryGetValue<IImmutableSet<SoftString>>(ResourceMetadataKeys.SchemeSet, out var schemeSet))
            {
                return metadata.SetItem(ResourceMetadataKeys.SchemeSet, schemeSet.Add(scheme));
            }
            else
            {
                return metadata.SetItem(ResourceMetadataKeys.SchemeSet, ImmutableHashSet.Create<SoftString>().Add(scheme));
            }
        }

        //public static bool CanGet(this ResourceMetadata metadata) => metadata.TryGetValue(ResourceMetadataKeys.CanGet, out bool value) && value;
        //public static bool CanPost(this ResourceMetadata metadata) => metadata.TryGetValue(ResourceMetadataKeys.CanPost, out bool value) && value;
        //public static bool CanPut(this ResourceMetadata metadata) => metadata.TryGetValue(ResourceMetadataKeys.CanPut, out bool value) && value;
        //public static bool CanDelete(this ResourceMetadata metadata) => metadata.TryGetValue(ResourceMetadataKeys.CanDelete, out bool value) && value;

        private static ResourceMetadata SetValue(this ResourceMetadata metadata, object value, [CallerMemberName] string memberName = null)
        {
            return metadata.Add(memberName, value);
        }
    }
}