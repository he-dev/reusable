using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.IOnymous
{
    public static class ResourceMetadataExtensions
    {
        /// <summary>
        /// Sets the specified item and uses an empty instance of ResourceMetadata if null.
        /// </summary>
        [NotNull]
        public static ResourceMetadata SetItemSafe([CanBeNull] this ResourceMetadata metadata, object value, [CallerMemberName] string key = null)
        {
            return (metadata ?? ResourceMetadata.Empty).SetItem(key, value);
        }

        // --
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

        public static T GetValueOrDefault<T>([CanBeNull] this ResourceMetadata metadata, T defaultValue = default, [CallerMemberName] string key = null)
        {
            return
                // ReSharper disable once AssignNullToNotNullAttribute - 'key' isn't null
                (metadata ?? ResourceMetadata.Empty).TryGetValue(key, out T value)
                    ? value
                    : defaultValue;
        }


        [NotNull, ItemCanBeNull]
        public static IEnumerable<SoftString> ProviderNames(this ResourceMetadata metadata)
        {
            yield return metadata.ProviderDefaultName();
            yield return metadata.ProviderCustomName();
        }

        #region Properties

        public static SoftString ProviderDefaultName(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault(SoftString.Empty);
        }
        
        public static ResourceMetadata ProviderDefaultName(this ResourceMetadata metadata, SoftString name)
        {
            return metadata.SetItemSafe(name);
        }

        public static SoftString ProviderCustomName(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault(SoftString.Empty);
        }
        
        public static ResourceMetadata ProviderCustomName(this ResourceMetadata metadata, SoftString name)
        {
            return metadata.SetItemSafe(name);
        }

        public static bool AllowRelativeUri(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault(false);
        }

        public static ResourceMetadata AllowRelativeUri(this ResourceMetadata metadata, bool value)
        {
            return metadata.SetItemSafe(value);
        }

        public static CancellationToken CancellationToken(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault(default(CancellationToken));
        }

        public static ResourceMetadata CancellationToken(this ResourceMetadata metadata, CancellationToken cancellationToken)
        {
            return metadata.SetItemSafe(cancellationToken);
        }

        // ---

        public static MimeType Format(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault(MimeType.Null);
        }

        public static ResourceMetadata Format(this ResourceMetadata metadata, MimeType format)
        {
            return metadata.SetItemSafe(format);
        }
        
        // ---

        public static Encoding Encoding(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault(System.Text.Encoding.UTF8);
        }

        public static ResourceMetadata Encoding(this ResourceMetadata metadata, Encoding encoding)
        {
            return metadata.SetItemSafe(encoding);
        }
        
        // ---

        public static IImmutableSet<SoftString> Schemes(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault((IImmutableSet<SoftString>)ImmutableHashSet<SoftString>.Empty);
        }

        public static ResourceMetadata Schemes(this ResourceMetadata metadata, params SoftString[] schemes)
        {
            return metadata.SetItemSafe((IImmutableSet<SoftString>)schemes.ToImmutableHashSet());
        }

        #endregion
    }
}