using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Extensions;

namespace Reusable.IOnymous
{
    public static class MetadataExtensions
    {
        #region Helpers

        /// <summary>
        /// Sets the specified item and uses the caller name as a key.
        /// </summary>
        public static Metadata SetItemAuto(this Metadata metadata, object value, [CallerMemberName] string key = null)
        {
            return metadata.SetItem(key, value);
        }

        // --
        private static bool TryGetValue<T>(this Metadata metadata, [NotNull] SoftString key, [CanBeNull] out T value)
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

        public static T GetValueOrDefault<T>(this Metadata metadata, T defaultValue = default, [CallerMemberName] string key = null)
        {
            return
                // ReSharper disable once AssignNullToNotNullAttribute - 'key' isn't null
                metadata.TryGetValue(key, out T value)
                    ? value
                    : defaultValue;
        }

        public static Metadata Union(this Metadata metadata, Metadata other)
        {
            return other.Aggregate(metadata, (current, i) => current.SetItem(i.Key, i.Value));
        }

        public static Metadata Union<T>(this MetadataScope<T> scope, Metadata other)
        {
            return scope.Metadata.Union(other);
        }

        #endregion

        #region Scope

        public static MetadataScope<T> For<T>(this Metadata metadata) //where T : IMetadataScope
        {
            return metadata.GetValueOrDefault(metadata, CreateScopeKey<T>());
        }

        public static Metadata For<T>(this Metadata metadata, Func<MetadataScope<T>, MetadataScope<T>> configureScope)
        {
            // There might already be a cope defined so get the current one first. 
            var scope = configureScope(metadata.For<T>().Metadata);
            return metadata.SetItemAuto(scope.Metadata, CreateScopeKey<T>());
        }

        private static string CreateScopeKey<TScope>()
        {
            return $"Scope:{typeof(TScope).ToPrettyString()}";
        }

        #endregion

        public static MetadataScope<IResourceInfo> Resource(this Metadata metadata)
        {
            return metadata.For<IResourceInfo>();
        }

        #region Properties

//        [NotNull, ItemCanBeNull]
//        public static IEnumerable<SoftString> ProviderNames(this ResourceMetadata metadata)
//        {
//            yield return metadata.ProviderDefaultName();
//            yield return metadata.ProviderCustomName();
//        }

        public static SoftString DefaultName(this Metadata metadata)
        {
            return metadata.GetValueOrDefault(SoftString.Empty);
        }

        public static Metadata DefaultName(this Metadata metadata, SoftString name)
        {
            return metadata.SetItemAuto(name);
        }

        public static SoftString CustomName(this Metadata metadata)
        {
            return metadata.GetValueOrDefault(SoftString.Empty);
        }

        public static Metadata CustomName(this Metadata metadata, SoftString name)
        {
            return metadata.SetItemAuto(name);
        }

        public static bool AllowRelativeUri(this Metadata metadata)
        {
            return metadata.GetValueOrDefault(false);
        }

        public static Metadata AllowRelativeUri(this Metadata metadata, bool value)
        {
            return metadata.SetItemAuto(value);
        }

        public static CancellationToken CancellationToken(this Metadata metadata)
        {
            return metadata.GetValueOrDefault(default(CancellationToken));
        }

        public static Metadata CancellationToken(this Metadata metadata, CancellationToken cancellationToken)
        {
            return metadata.SetItemAuto(cancellationToken);
        }

        // ---

        public static MimeType Format(this MetadataScope<IResourceInfo> scope)
        {
            return scope.Metadata.GetValueOrDefault(MimeType.Null);
        }

        public static Metadata Format(this MetadataScope<IResourceInfo> scope, MimeType format)
        {
            return scope.Metadata.SetItemAuto(format);
        }

        // ---

        public static Encoding Encoding(this Metadata metadata)
        {
            return metadata.GetValueOrDefault(System.Text.Encoding.UTF8);
        }

        public static Metadata Encoding(this Metadata metadata, Encoding encoding)
        {
            return metadata.SetItemAuto(encoding);
        }

        // ---

        public static IImmutableSet<SoftString> Schemes(this Metadata metadata)
        {
            return metadata.GetValueOrDefault((IImmutableSet<SoftString>)ImmutableHashSet<SoftString>.Empty);
        }

        public static Metadata Schemes(this Metadata metadata, params SoftString[] schemes)
        {
            return metadata.SetItemAuto((IImmutableSet<SoftString>)schemes.ToImmutableHashSet());
        }

        // ---

        public static Type Type(this Metadata metadata)
        {
            return metadata.GetValueOrDefault(System.Type.Missing.GetType());
        }

        public static Metadata Type(this Metadata metadata, Type type)
        {
            return metadata.SetItemAuto(type);
        }

        // ---

        public static string InternalName(this MetadataScope<IResourceInfo> scope)
        {
            return scope.Metadata.GetValueOrDefault(default(string));
        }

        public static MetadataScope<IResourceInfo> InternalName(this MetadataScope<IResourceInfo> scope, string name)
        {
            return scope.Metadata.SetItemAuto(name);
        }

        #endregion
    }

    public interface IMetadataScope { }
}