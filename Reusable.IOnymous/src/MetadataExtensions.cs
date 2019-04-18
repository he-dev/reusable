using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Extensions;

namespace Reusable.IOnymous
{
    public delegate MetadataScope<T> ConfigureMetadataScopeCallback<T>(MetadataScope<T> scope);

    public static class MetadataExtensions
    {
        #region Helpers

        /// <summary>
        /// Sets the specified item and uses the caller name as a key.
        /// </summary>
        public static Metadata SetItemWithCallerName(this Metadata metadata, object value, [CallerMemberName] string key = null)
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

        public static T GetValueByCallerName<T>(this Metadata metadata, T defaultValue = default, [CallerMemberName] string key = null)
        {
            return
                // ReSharper disable once AssignNullToNotNullAttribute - 'key' isn't null
                metadata.TryGetValue(key, out T value)
                    ? value
                    : defaultValue;
        }

        public static Metadata Union(this Metadata metadata, Metadata other)
        {
            //return other.Aggregate(metadata, (current, i) => current.SetItem(i.Key, i.Value));

            var result = metadata;

            foreach (var item in other)
            {
                if (item.Value is Metadata otherScope)
                {
                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (result.TryGetValue(item.Key, out Metadata currentScope))
                    {
                        result = result.SetItem(item.Key, currentScope.Aggregate(otherScope, (current, i) => current.SetItem(i.Key, i.Value)));
                    }
                    else
                    {
                        result = result.SetItem(item.Key, otherScope);
                    }
                }
                else
                {
                    result = result.SetItem(item.Key, item.Value);
                }
            }

            return result;
        }

//        public static MetadataScope<T> Union<T>(this MetadataScope<T> scope, Metadata other)
//        {
//            return scope.Metadata.Union(other);
//        }

        #endregion

        #region Scope

        public static MetadataScope<T> For<T>(this Metadata metadata)
        {
            var scopeKey = CreateMetadataScopeKey<T>();
            return metadata.GetValueByCallerName(metadata, scopeKey);
        }

        public static Metadata For<T>(this Metadata metadata, ConfigureMetadataScopeCallback<T> configureScope)
        {
            // There might already be a cope defined so get the current one first. 
            var scope = configureScope(metadata.For<T>().Metadata);
            return metadata.SetItemWithCallerName(scope.Metadata, CreateMetadataScopeKey<T>());
        }

        private static string CreateMetadataScopeKey<TScope>()
        {
            return $"Scope:{typeof(TScope).ToPrettyString()}";
        }

        #endregion

        #region General properties

        public static bool AllowRelativeUri(this Metadata metadata)
        {
            return metadata.GetValueByCallerName(false);
        }

        public static Metadata AllowRelativeUri(this Metadata metadata, bool value)
        {
            return metadata.SetItemWithCallerName(value);
        }

        public static CancellationToken CancellationToken(this Metadata metadata)
        {
            return metadata.GetValueByCallerName(default(CancellationToken));
        }

        public static Metadata CancellationToken(this Metadata metadata, CancellationToken cancellationToken)
        {
            return metadata.SetItemWithCallerName(cancellationToken);
        }

        // ---

        public static Encoding Encoding(this Metadata metadata)
        {
            return metadata.GetValueByCallerName(System.Text.Encoding.UTF8);
        }

        public static Metadata Encoding(this Metadata metadata, Encoding encoding)
        {
            return metadata.SetItemWithCallerName(encoding);
        }

        // ---

        public static IImmutableSet<SoftString> Schemes(this Metadata metadata)
        {
            return metadata.GetValueByCallerName((IImmutableSet<SoftString>)ImmutableHashSet<SoftString>.Empty);
        }

        public static Metadata Schemes(this Metadata metadata, params SoftString[] schemes)
        {
            return metadata.SetItemWithCallerName((IImmutableSet<SoftString>)schemes.ToImmutableHashSet());
        }

        // ---        

        #endregion

        #region Provider properties

        public static MetadataScope<IResourceProvider> Provider(this Metadata metadata)
        {
            return metadata.For<IResourceProvider>();
        }

        public static Metadata Provider(this Metadata metadata, ConfigureMetadataScopeCallback<IResourceProvider> scope)
        {
            return metadata.For(scope);
        }

        public static SoftString DefaultName(this MetadataScope<IResourceProvider> scope)
        {
            return scope.Metadata.GetValueByCallerName(SoftString.Empty);
        }

        public static Metadata DefaultName(this MetadataScope<IResourceProvider> scope, SoftString name)
        {
            return scope.Metadata.SetItemWithCallerName(name);
        }

        public static SoftString CustomName(this MetadataScope<IResourceProvider> scope)
        {
            return scope.Metadata.GetValueByCallerName(SoftString.Empty);
        }

        public static Metadata CustomName(this MetadataScope<IResourceProvider> scope, SoftString name)
        {
            return scope.Metadata.SetItemWithCallerName(name);
        }

        #endregion

        #region Resource properties

        public static MetadataScope<IResourceInfo> Resource(this Metadata metadata)
        {
            return metadata.For<IResourceInfo>();
        }

        public static Metadata Resource(this Metadata metadata, ConfigureMetadataScopeCallback<IResourceInfo> scope)
        {
            return metadata.For(scope);
        }

        public static MimeType Format(this MetadataScope<IResourceInfo> scope)
        {
            return scope.Metadata.GetValueByCallerName(MimeType.Null);
        }

        public static Metadata Format(this MetadataScope<IResourceInfo> scope, MimeType format)
        {
            return scope.Metadata.SetItemWithCallerName(format);
        }

        public static string InternalName(this MetadataScope<IResourceInfo> scope)
        {
            return scope.Metadata.GetValueByCallerName(default(string));
        }

        public static MetadataScope<IResourceInfo> InternalName(this MetadataScope<IResourceInfo> scope, string name)
        {
            return scope.Metadata.SetItemWithCallerName(name);
        }

        public static Type Type(this MetadataScope<IResourceInfo> scope)
        {
            return scope.Metadata.GetValueByCallerName(System.Type.Missing.GetType());
        }

        public static Metadata Type(this MetadataScope<IResourceInfo> scope, Type type)
        {
            return scope.Metadata.SetItemWithCallerName(type);
        }

        #endregion
    }
}