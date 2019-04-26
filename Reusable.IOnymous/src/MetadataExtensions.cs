using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.IOnymous
{
    public delegate Metadata<T> ConfigureMetadataScopeCallback<T>(Metadata<T> scope);

    public static class MetadataExtensions
    {
        #region Helpers

        public static T GetItemByCallerName<T>(this Metadata metadata, T defaultValue = default, [CallerMemberName] string key = null)
        {
            return
                // ReSharper disable once AssignNullToNotNullAttribute - 'key' isn't null
                metadata.TryGetValue(key, out T value)
                    ? value
                    : defaultValue;
        }

        public static bool TryGetValue<T>(this Metadata metadata, [NotNull] SoftString key, [CanBeNull] out T value)
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

        /// <summary>
        /// Sets the specified item and uses the caller name as a key.
        /// </summary>
        public static Metadata SetItemByCallerName(this Metadata metadata, object value, [CallerMemberName] string key = null)
        {
            return metadata.SetItem(key, value);
        }

        [MustUseReturnValue]
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

        /// <summary>
        /// Gets Metadata scope or an empty one if there is none yet.
        /// </summary>
        [MustUseReturnValue]
        public static Metadata<T> Scope<T>(this Metadata metadata)
        {
            return metadata.TryGetValue(Metadata<T>.Key, out Metadata value)
                ? value
                : Metadata.Empty;
        }

        /// <summary>
        /// Sets Metadata scope.
        /// </summary>
        [MustUseReturnValue]
        public static Metadata Scope<T>(this Metadata metadata, [NotNull] ConfigureMetadataScopeCallback<T> configureScope)
        {
            if (configureScope == null) throw new ArgumentNullException(nameof(configureScope));
            
            // There might already be a cope defined so get the current one first. 
            var scope = configureScope(metadata.Scope<T>().Value);
            return metadata.SetItem(Metadata<T>.Key, scope.Value);
        }

        #endregion

        #region General properties

        public static bool AllowRelativeUri(this Metadata metadata)
        {
            return metadata.GetItemByCallerName(false);
        }

        public static Metadata AllowRelativeUri(this Metadata metadata, bool value)
        {
            return metadata.SetItemByCallerName(value);
        }

        public static CancellationToken CancellationToken(this Metadata metadata)
        {
            return metadata.GetItemByCallerName(default(CancellationToken));
        }

        public static Metadata CancellationToken(this Metadata metadata, CancellationToken cancellationToken)
        {
            return metadata.SetItemByCallerName(cancellationToken);
        }

        // ---

        public static Encoding Encoding(this Metadata metadata)
        {
            return metadata.GetItemByCallerName(System.Text.Encoding.UTF8);
        }

        public static Metadata Encoding(this Metadata metadata, Encoding encoding)
        {
            return metadata.SetItemByCallerName(encoding);
        }

        // ---

        public static IImmutableSet<SoftString> Schemes(this Metadata metadata)
        {
            return metadata.GetItemByCallerName((IImmutableSet<SoftString>)ImmutableHashSet<SoftString>.Empty);
        }

        public static Metadata Schemes(this Metadata metadata, params SoftString[] schemes)
        {
            return metadata.SetItemByCallerName((IImmutableSet<SoftString>)schemes.ToImmutableHashSet());
        }

        // ---        

        #endregion

        #region Provider properties

        public static Metadata<IResourceProvider> Provider(this Metadata metadata)
        {
            return metadata.Scope<IResourceProvider>();
        }

        public static Metadata Provider(this Metadata metadata, ConfigureMetadataScopeCallback<IResourceProvider> scope)
        {
            return metadata.Scope(scope);
        }

        public static SoftString DefaultName(this Metadata<IResourceProvider> scope)
        {
            return scope.Value.GetItemByCallerName(SoftString.Empty);
        }

        public static Metadata<IResourceProvider> DefaultName(this Metadata<IResourceProvider> scope, SoftString name)
        {
            return scope.Value.SetItemByCallerName(name);
        }

        public static SoftString CustomName(this Metadata<IResourceProvider> scope)
        {
            return scope.Value.GetItemByCallerName(SoftString.Empty);
        }

        public static Metadata<IResourceProvider> CustomName(this Metadata<IResourceProvider> scope, SoftString name)
        {
            return scope.Value.SetItemByCallerName(name);
        }

        #endregion

        #region Resource properties

        public static Metadata<IResourceInfo> Resource(this Metadata metadata)
        {
            return metadata.Scope<IResourceInfo>();
        }

        public static Metadata Resource(this Metadata metadata, ConfigureMetadataScopeCallback<IResourceInfo> scope)
        {
            return metadata.Scope(scope);
        }

        public static MimeType Format(this Metadata<IResourceInfo> scope)
        {
            return scope.Value.GetItemByCallerName(MimeType.Null);
        }

        public static Metadata Format(this Metadata<IResourceInfo> scope, MimeType format)
        {
            return scope.Value.SetItemByCallerName(format);
        }

        public static string InternalName(this Metadata<IResourceInfo> scope)
        {
            return scope.Value.GetItemByCallerName(default(string));
        }

        public static Metadata<IResourceInfo> InternalName(this Metadata<IResourceInfo> scope, string name)
        {
            return scope.Value.SetItemByCallerName(name);
        }

        public static Type Type(this Metadata<IResourceInfo> scope)
        {
            return scope.Value.GetItemByCallerName(System.Type.Missing.GetType());
        }

        public static Metadata Type(this Metadata<IResourceInfo> scope, Type type)
        {
            return scope.Value.SetItemByCallerName(type);
        }

        #endregion
    }
}