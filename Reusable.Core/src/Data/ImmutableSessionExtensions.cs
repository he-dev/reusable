using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Reusable.Data
{
    public static class ImmutableSessionExtensions
    {
        [NotNull]
        public static IImmutableSession ThisOrEmpty(this IImmutableSession session) => session ?? ImmutableSession.Empty;

        #region Helpers

        [DebuggerStepThrough]
        [MustUseReturnValue]
        public static T GetItemOrDefault<T>(this IImmutableSession session, Key<T> key, T defaultValue = default)
        {
            return session.TryGetItem(key, out var item) ? item : defaultValue;
        }

        public static bool TryGetItem<T>(this IImmutableSession session, Key<T> key, out T value)
        {
            if (session.TryGetValue(key, out var item))
            {
                if (item is T t)
                {
                    value = t;
                    return true;
                }
            }

            value = default;
            return false;
        }

//        public static IImmutableSession SetItem<T>(this IImmutableSession session, Key<T> key, T value)
//        {
//            return session.SetItem(key, value);
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
    }
}