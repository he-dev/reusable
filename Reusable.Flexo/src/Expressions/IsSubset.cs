using System.Collections.Generic;
using System.Linq.Custom;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class IsSubset : CollectionExtension<bool>
    {
        public IsSubset(ILogger<IsSubset> logger) : base(logger, nameof(IsSubset)) { }

        public IEnumerable<IExpression> Values
        {
            get => ThisInner ?? ThisOuter;
            set => ThisInner = value;
        }

        [JsonRequired]
        public List<IExpression> Of { get; set; }

        public string Comparer { get; set; }

        protected override Constant<bool> InvokeCore(IImmutableContainer context)
        {
            var first = Values.Invoke().Values<object>();
            var second = Of.Invoke().Values<object>();
            var comparer = Scope.GetComparerOrDefault(Comparer);
            return (Name, first.IsSubsetOf(second, comparer));
        }
    }
}