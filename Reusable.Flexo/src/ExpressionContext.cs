using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Reusable.Data;
using Reusable.Quickey;

namespace Reusable.Flexo
{
    [UseMember]
    [PlainSelectorFormatter]
    public class ExpressionContext : SelectorBuilder<ExpressionContext>
    {
        public static IImmutableContainer Default =>
            ImmutableContainer
                .Empty
                .SetItem(EqualityComparers, ImmutableDictionary<SoftString, IEqualityComparer<object>>.Empty)
                .SetItem(References, ImmutableDictionary<SoftString, IExpression>.Empty)
                .SetItem(DebugView, Node.Create(ExpressionDebugView.Root))
                .WithDefaultEqualityComparer()
                .WithDefaultComparer()
                .WithSoftStringComparer()
                .WithRegexComparer();

        /// <summary>
        /// Gets or sets extension value.
        /// </summary>
        public static readonly Selector<object> ThisOuter = Select(() => ThisOuter);

        /// <summary>
        /// Gets or sets collection item.
        /// </summary>
        public static readonly Selector<object> Item = Select(() => Item);

        public static readonly Selector<IImmutableContainer> Parent = Select(() => Parent);
        
        public static readonly Selector<IImmutableDictionary<SoftString, IEqualityComparer<object>>> EqualityComparers = Select(() => EqualityComparers);
        
        public static readonly Selector<IImmutableDictionary<SoftString, IComparer<object>>> Comparers = Select(() => Comparers);

        public static readonly Selector<IImmutableDictionary<SoftString, IExpression>> References = Select(() => References);

        public static readonly Selector<Node<ExpressionDebugView>> DebugView = Select(() => DebugView);

        public static readonly Selector<CancellationToken> CancellationToken = Select(() => CancellationToken);
    }
}