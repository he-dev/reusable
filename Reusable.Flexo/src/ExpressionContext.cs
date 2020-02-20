using System.Collections.Generic;
using System.Threading;
using Reusable.Data;
using Reusable.Flexo.Abstractions;
using Reusable.Flexo.Containers;
using Reusable.Quickey;

namespace Reusable.Flexo
{
    [UseNamespace(Keywords.Flexo), UseMember]
    [JoinSelectorTokens]
    public static partial class ExpressionContext
    {
        public static IImmutableContainer Default =>
            ImmutableContainer
                .Empty
                .SetItem(Id, Keywords.Main)
                .SetItem(EqualityComparers, new EqualityComparerContainer
                {
                    [Keywords.Default] = EqualityComparer<object>.Default,
                    ["SoftString"] = SoftString.Comparer.Objectify<string>()
                })
                .SetItem(Comparers, new ComparerContainer
                {
                    [Keywords.Default] = Comparer<object>.Default
                })
                .SetItem(InvokeLog, Node.Create((IExpression)Constant.Single<object>("ExpressionInvokeLog")));

        private static readonly From<ExecutionContext> This = From<ExecutionContext>.This!;

        /// <summary>
        /// Gets or sets extension value.
        /// </summary>
        public static readonly Selector<IConstant> Arg = This.Select(() => Arg);

        /// <summary>
        /// Gets or sets scope id.
        /// </summary>
        public static readonly Selector<string> Id = This.Select(() => Id);

        /// <summary>
        /// Gets or sets the parent scope.
        /// </summary>
        public static readonly Selector<IImmutableContainer> Parent = This.Select(() => Parent);
        
        public static readonly Selector<IContainer<string, IEqualityComparer<object>>> EqualityComparers = This.Select(() => EqualityComparers);

        public static readonly Selector<IContainer<string, IComparer<object>>> Comparers = This.Select(() => Comparers);

        public static readonly Selector<IContainer<string, Package>> Packages = This.Select(() => Packages);

        public static readonly Selector<Node<IExpression>> InvokeLog = This.Select(() => InvokeLog);

        public static readonly Selector<CancellationToken> CancellationToken = This.Select(() => CancellationToken);


    }
    
    public static class Keywords
    {
        public const string Flexo = nameof(Flexo);
        public const string Main = nameof(Main);
        public const string Default = nameof(Default);
    }
}