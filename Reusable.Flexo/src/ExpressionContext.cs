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
                .SetItem(EqualityComparers, new Dictionary<SoftString, IEqualityComparer<object>>
                {
                    ["Default"] = EqualityComparer<object>.Default,
                    ["SoftString"] = SoftString.Comparer.Objectify<string>()
                })
                .SetItem(Comparers, new Dictionary<SoftString, IComparer<object>>
                {
                    ["Default"] = Comparer<object>.Default
                })
                .SetItem(InvokeLog, Node.Create((IExpression)Constant.FromValue<object>("ExpressionInvokeLog")))
                .SetItem(GetPackageFunc, (string packageId) => default);

        private static readonly From<ExecutionContext> This = From<ExecutionContext>.This!;

        /// <summary>
        /// Gets or sets extension value.
        /// </summary>
        public static readonly Selector<object> Arg = This.Select(() => Arg);

        /// <summary>
        /// Gets or sets collection item.
        /// </summary>
        public static readonly Selector<IImmutableContainer> Parent = This.Select(() => Parent);

        public static readonly Selector<IDictionary<SoftString, IEqualityComparer<object>>> EqualityComparers = This.Select(() => EqualityComparers);

        public static readonly Selector<IDictionary<SoftString, IComparer<object>>> Comparers = This.Select(() => Comparers);

        public static readonly Selector<GetPackageFunc> GetPackageFunc = This.Select(() => GetPackageFunc);

        public static readonly Selector<Node<IExpression>> InvokeLog = This.Select(() => InvokeLog);

        public static readonly Selector<CancellationToken> CancellationToken = This.Select(() => CancellationToken);

        public static string ToInvokeLogString(this IImmutableContainer context, RenderTreeNodeValueDelegate<IExpression, NodePlainView> template)
        {
            return context.FindItem(InvokeLog).Views(template).Render();
        }

        public static IConstant InvokePackage(this IImmutableContainer context, string packageId)
        {
            return context.FindPackage(packageId).Invoke(context);
        }
    }

    public delegate IExpression? GetPackageFunc(string packageId);
}