using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Reusable.Data
{
    //public delegate ImmutableSessionSetter<T> ConfigureMetadataScopeCallback<T>(ImmutableSessionSetter<T> scope) where T : INestedSession;

    

    public static class ImmutableSessionExtensions
    {
        [NotNull]
        public static IImmutableSession ThisOrEmpty(this IImmutableSession session) => session ?? ImmutableSession.Empty;
        
        #region Helpers
//
//        public static T GetItemByCallerName<T>([NotNull] this IImmutableSession metadata, T defaultValue = default, [CallerMemberName] string key = null)
//        {
//            if (metadata == null) throw new ArgumentNullException(nameof(metadata));
//            
//            return
//                // ReSharper disable once AssignNullToNotNullAttribute - 'key' isn't null
//                metadata.TryGetValue(key, out T value)
//                    ? value
//                    : defaultValue;
//        }

        public static bool TryGetValue<T>([NotNull] this IImmutableSession metadata, [NotNull] SoftString key, [CanBeNull] out T value)
        {
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));
            
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

//        /// <summary>
//        /// Sets the specified item and uses the caller name as a key.
//        /// </summary>
//        public static IImmutableSession SetItemByCallerName([NotNull] this IImmutableSession metadata, object value, [CallerMemberName] string key = null)
//        {
//            if (metadata == null) throw new ArgumentNullException(nameof(metadata));
//            
//            return metadata.SetItem(key, value);
//        }

//        [MustUseReturnValue]
//        public static Metadata Union(this Metadata metadata, Metadata other)
//        {
//            //return other.Aggregate(metadata, (current, i) => current.SetItem(i.Key, i.Value));
//
//            var result = metadata;
//
//            foreach (var item in other)
//            {
//                if (item.Value is Metadata otherScope)
//                {
//                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
//                    if (result.TryGetValue(item.Key, out Metadata currentScope))
//                    {
//                        result = result.SetItem(item.Key, currentScope.Aggregate(otherScope, (current, i) => current.SetItem(i.Key, i.Value)));
//                    }
//                    else
//                    {
//                        result = result.SetItem(item.Key, otherScope);
//                    }
//                }
//                else
//                {
//                    result = result.SetItem(item.Key, item.Value);
//                }
//            }
//
//            return result;
//        }

        //        public static MetadataScope<T> Union<T>(this MetadataScope<T> scope, Metadata other)
        //        {
        //            return scope.Metadata.Union(other);
        //        }

        #endregion

        #region Scope

//        /// <summary>
//        /// Gets Metadata scope or an empty one if there is none yet.
//        /// </summary>
//        [MustUseReturnValue]
//        public static ImmutableSessionGetter<T> Scope<T>(this IImmutableSession session) where T : INestedSession
//        {
//            var scope = session.TryGetValue(ImmutableSession<T>.Key, out IImmutableSession value)
//                ? value
//                : ImmutableSession.Empty;
//            
//            return new ImmutableSessionGetter<T>(scope);
//        }
//
//        // session[NameSession]
//        /// <summary>
//        /// Sets Metadata scope.
//        /// </summary>
//        [MustUseReturnValue]
//        public static IImmutableSession Scope<T>(this IImmutableSession session, [NotNull] ConfigureMetadataScopeCallback<T> configureScope) where T : INestedSession
//        {
//            if (configureScope == null) throw new ArgumentNullException(nameof(configureScope));
//
//            // There might already be a cope defined so get the current one first. 
//            var sessionSetter = configureScope(new ImmutableSessionSetter<T>(session.Scope<T>().Value));
//            return session.SetItem(ImmutableSession<T>.Key, sessionSetter.Value);
//        }

        #endregion
     
    }
}