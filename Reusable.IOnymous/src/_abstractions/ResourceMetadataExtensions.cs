using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using JetBrains.Annotations;
using Newtonsoft.Json;

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

        public static T GetValueOrDefault<T>([CanBeNull] this ResourceMetadata metadata, [NotNull] SoftString key, T defaultValue = default)
        {
            return 
                metadata.TryGetValue(key, out T value) 
                    ? value 
                    : defaultValue;
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
        
        private static ResourceMetadata SetValue(this ResourceMetadata metadata, object value, [CallerMemberName] string memberName = null)
        {
            return metadata.Add(memberName, value);
        }
        
        // ---
        
        
        public static CancellationToken CancellationToken(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault(nameof(CancellationToken), System.Threading.CancellationToken.None);
        }

        public static ResourceMetadata CancellationToken(this ResourceMetadata metadata, CancellationToken cancellationToken)
        {
            return metadata.SetItem(nameof(CancellationToken), cancellationToken);
        }
        
        // ---
        
        public static string RelativeUriScheme(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault<string>(nameof(RelativeUriScheme));
        }

        public static ResourceMetadata RelativeUriScheme(this ResourceMetadata metadata, string relativeUriScheme)
        {
            return metadata.SetItem(nameof(RelativeUriScheme), relativeUriScheme);
        }
        
        // ---
        
        public static JsonSerializer JsonSerializer(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault(nameof(JsonSerializer), new JsonSerializer());
        }

        public static ResourceMetadata JsonSerializer(this ResourceMetadata metadata, JsonSerializer jsonSerializer)
        {
            return metadata.SetItem(nameof(JsonSerializer), jsonSerializer);
        }
        
        // ---
        
        public static MimeType Format(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault(nameof(Format), MimeType.Null);
        }

        public static ResourceMetadata Format(this ResourceMetadata metadata, MimeType format)
        {
            return metadata.SetItem(nameof(Format), format);
        }
    }
}