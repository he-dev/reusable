using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Quickey;

namespace Reusable.Data
{
    public static class ImmutableSessionExtensions
    {
        [NotNull]
        public static IImmutableSession ThisOrEmpty(this IImmutableSession session) => session ?? ImmutableSession.Empty;

        #region Helpers

        [DebuggerStepThrough]
        [MustUseReturnValue]
        public static T GetItemOrDefault<T>(this IImmutableSession session, Selector<T> key, T defaultValue = default)
        {
            return session.TryGetItem(key, out var item) ? item : defaultValue;
        }

        public static bool TryGetItem<T>(this IImmutableSession session, Selector<T> key, out T value)
        {
            if (session.TryGetValue(key.ToString(), out var item))
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

        public static IImmutableSession SetItem<T>(this IImmutableSession session, Selector<T> key, T value)
        {
            return session.SetItem(key.ToString(), value);
        }

        public static Func<IImmutableSession, IImmutableSession> MergeFunc(IImmutableSession other)
        {
            return current =>
            {
                foreach (var (key, value) in other)
                {
                    current = current.SetItem(key, value);
                }

                return current;
            };
        }

//        [MustUseReturnValue]
//        public static IImmutableSession Union(this IImmutableSession metadata, IImmutableSession other)
//        {
//            //return other.Aggregate(metadata, (current, i) => current.SetItem(i.Key, i.Value));
//
//            var result = metadata;
//
//            foreach (var item in other)
//            {
//                if (item.Value is IImmutableSession otherScope)
//                {
//                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
//                    if (result.TryGetValue(item.Key, out IImmutableSession currentScope))
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

        public static IImmutableSession SetWhen(this IImmutableSession session, Func<IImmutableSession, bool> canSet, Func<IImmutableSession, IImmutableSession> set)
        {
            return
                canSet(session)
                    ? set(session)
                    : session;
        }
    }
}