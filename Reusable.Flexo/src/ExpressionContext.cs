using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Flexo.Abstractions;
using Reusable.Quickey;

namespace Reusable.Flexo
{
    [UseNamespace("Flexo"), UseMember]
    [PlainSelectorFormatter]
    public static partial class ExpressionContext
    {
        public static IImmutableContainer Default =>
            ImmutableContainer
                .Empty
                .SetItem(EqualityComparers, ImmutableDictionary<SoftString, IEqualityComparer<object>>.Empty)
                .SetItem(Packages, ImmutableDictionary<SoftString, IExpression>.Empty)
                .SetItem(InvokeLog, Node.Create((IExpression)Constant.FromValue<object>("ExpressionInvokeLog")))
                .SetItem(GetPackageFunc, (string packageId) => default)
                .SetDefaultComparer()
                .SetEqualityComparer("Default", EqualityComparer<object>.Default)
                .SetEqualityComparer<string>(nameof(SoftString), SoftString.Comparer);
        //.WithRegexComparer();

        private static readonly From<ExecutionContext> This = From<ExecutionContext>.This;

        /// <summary>
        /// Gets or sets extension value.
        /// </summary>
        public static readonly Selector<object> Arg = This.Select(() => Arg);

        /// <summary>
        /// Gets or sets collection item.
        /// </summary>
        //public static readonly Selector<object> Item = Select(() => Item);
        public static readonly Selector<IImmutableContainer> Parent = This.Select(() => Parent);

        public static readonly Selector<IImmutableDictionary<SoftString, IEqualityComparer<object>>> EqualityComparers = This.Select(() => EqualityComparers);

        public static readonly Selector<IImmutableDictionary<SoftString, IComparer<object>>> Comparers = This.Select(() => Comparers);

        public static readonly Selector<IImmutableDictionary<SoftString, IExpression>> Packages = This.Select(() => Packages);

        public static readonly Selector<GetPackageFunc> GetPackageFunc = This.Select(() => GetPackageFunc);

        public static readonly Selector<Node<IExpression>> InvokeLog = This.Select(() => InvokeLog);

        public static readonly Selector<CancellationToken> CancellationToken = This.Select(() => CancellationToken);

        public static string ToInvokeLogString(this IImmutableContainer context, RenderTreeNodeValueDelegate<IExpression, NodePlainView> template)
        {
            return context.FindItem(InvokeLog).Views(template).Render();
        }

        public static IExpression FindPackage(this IImmutableContainer context, string packageId)
        {
            foreach (var getPackage in context.FindItems(GetPackageFunc))
            {
                if (getPackage(packageId) is {} package)
                {
                    return package;
                }
            }

            throw DynamicException.Create("PackageNotFound", $"Could not find package '{packageId}'.");
        }

        public static IConstant InvokePackage(this IImmutableContainer context, string packageId)
        {
            return context.FindPackage(packageId).Invoke(context);
        }
    }

    public delegate IExpression? GetPackageFunc(string packageId);
}