using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Reusable.Collections;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Flexo.Abstractions;
using linq = System.Linq.Expressions;

namespace Reusable.Flexo
{
    public static partial class ExpressionContext
    {
        #region Comparers

        public static IImmutableContainer SetDefaultComparer(this IImmutableContainer context)
        {
            return context.UpdateItem(Comparers, x => x.SetItem(nameof(Default)!, Comparer<object>.Default));
        }

//        public static IImmutableContainer AddComparer<T>(this IImmutableContainer context, string name, IComparer<T> comparer)
//        {
//            return context.UpdateItem(ExpressionContext.Comparers, x => x.SetItem(name, comparer));
//        }

        public static IImmutableContainer SetEqualityComparer<T>(this IImmutableContainer context, string name, IEqualityComparer<T> comparer)
        {
            return context.UpdateItem(EqualityComparers, x => x.SetItem(name!, EqualityComparerFactory<object>.Create
            (
                equals: (left, right) => comparer.Equals((T)left, (T)right),
                getHashCode: (obj) => comparer.GetHashCode((T)obj)
            )));
        }

        #endregion

        public static IImmutableContainer SetPackages(this IImmutableContainer context, IEnumerable<IExpression> expressions)
        {
            var packages = expressions.Aggregate
            (
                context.GetItemOrDefault(Packages, ImmutableDictionary<SoftString, IExpression>.Empty),
                (current, next) => next switch
                {
                    Package p => current.SetItem(p.Id, p),
                    {} x => throw DynamicException.Create("InvalidExpression", $"{x.Id} is not a package."),
                    _ => throw DynamicException.Create("PackageNull", "Package must not be null.")
                }
            );

            return context.SetItem(Packages, packages);
        }


        public static Node<IExpression> GetInvokeLog(this IImmutableContainer context)
        {
            return context.FindItem(InvokeLog);
        }

        public static IImmutableContainer SetInvokeLog(this IImmutableContainer context, Node<IExpression> invokeLog)
        {
            return context.SetItem(InvokeLog, invokeLog);
        }
        
        public static IImmutableContainer SetTryGetPackageFunc(this IImmutableContainer context, GetPackageFunc tryGetPackageFunc)
        {
            return context.SetItem(GetPackageFunc, tryGetPackageFunc);
        }

        
    }
}