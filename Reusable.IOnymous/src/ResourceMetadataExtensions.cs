using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Extensions;

namespace Reusable.IOnymous
{
    public static class ResourceMetadataExtensions
    {
        #region Helpers

        /// <summary>
        /// Sets the specified item and uses the caller name as a key.
        /// </summary>
        public static ResourceMetadata SetItemAuto(this ResourceMetadata metadata, object value, [CallerMemberName] string key = null)
        {
            return metadata.SetItem(key, value);
        }

        // --
        private static bool TryGetValue<T>(this ResourceMetadata metadata, [NotNull] SoftString key, [CanBeNull] out T value)
        {
            if (metadata.TryGetValue(key, out var x) && x is T result)
            {
                value = result;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public static T GetValueOrDefault<T>(this ResourceMetadata metadata, T defaultValue = default, [CallerMemberName] string key = null)
        {
            return
                // ReSharper disable once AssignNullToNotNullAttribute - 'key' isn't null
                metadata.TryGetValue(key, out T value)
                    ? value
                    : defaultValue;
        }

        #endregion

        #region Scope

        public static ResourceMetadataScope<T> Scope<T>(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault(ResourceMetadata.Empty, CreateScopeKey<T>());
        }

        public static ResourceMetadata Scope<T>(this ResourceMetadata metadata, Func<ResourceMetadataScope<T>, ResourceMetadataScope<T>> configureScope)
        {
            // There might already be a cope defined so get the current one first. 
            var scope = configureScope(metadata.Scope<T>().Metadata);
            return metadata.SetItemAuto(scope.Metadata, CreateScopeKey<T>());
        }

        private static string CreateScopeKey<TScope>()
        {
            return $"Scope:{typeof(TScope).ToPrettyString()}";
        }

        #endregion

        #region Properties

        [NotNull, ItemCanBeNull]
        public static IEnumerable<SoftString> ProviderNames(this ResourceMetadata metadata)
        {
            yield return metadata.ProviderDefaultName();
            yield return metadata.ProviderCustomName();
        }

        public static SoftString ProviderDefaultName(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault(SoftString.Empty);
        }

        public static ResourceMetadata ProviderDefaultName(this ResourceMetadata metadata, SoftString name)
        {
            return metadata.SetItemAuto(name);
        }

        public static SoftString ProviderCustomName(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault(SoftString.Empty);
        }

        public static ResourceMetadata ProviderCustomName(this ResourceMetadata metadata, SoftString name)
        {
            return metadata.SetItemAuto(name);
        }

        public static bool AllowRelativeUri(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault(false);
        }

        public static ResourceMetadata AllowRelativeUri(this ResourceMetadata metadata, bool value)
        {
            return metadata.SetItemAuto(value);
        }

        public static CancellationToken CancellationToken(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault(default(CancellationToken));
        }

        public static ResourceMetadata CancellationToken(this ResourceMetadata metadata, CancellationToken cancellationToken)
        {
            return metadata.SetItemAuto(cancellationToken);
        }

        // ---

        public static MimeType Format(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault(MimeType.Null);
        }

        public static ResourceMetadata Format(this ResourceMetadata metadata, MimeType format)
        {
            return metadata.SetItemAuto(format);
        }

        // ---

        public static Encoding Encoding(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault(System.Text.Encoding.UTF8);
        }

        public static ResourceMetadata Encoding(this ResourceMetadata metadata, Encoding encoding)
        {
            return metadata.SetItemAuto(encoding);
        }

        // ---

        public static IImmutableSet<SoftString> Schemes(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault((IImmutableSet<SoftString>)ImmutableHashSet<SoftString>.Empty);
        }

        public static ResourceMetadata Schemes(this ResourceMetadata metadata, params SoftString[] schemes)
        {
            return metadata.SetItemAuto((IImmutableSet<SoftString>)schemes.ToImmutableHashSet());
        }

        #endregion
    }
}