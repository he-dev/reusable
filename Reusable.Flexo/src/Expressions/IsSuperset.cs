using System.Collections.Generic;
using System.Linq.Custom;
using Newtonsoft.Json;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class IsSuperset : CollectionExtension<bool>
    {
        public IsSuperset(ILogger<IsSuperset> logger) : base(logger, nameof(IsSuperset)) { }

        public IEnumerable<IExpression> Values
        {
            get => ThisInner ?? ThisOuter;
            set => ThisInner = value;
        }

        [JsonRequired]
        public List<IExpression> Of { get; set; }

        public string Comparer { get; set; }

        protected override Constant<bool> InvokeCore()
        {
            var first = Values.Invoke().Values<object>();
            var second = Of.Invoke().Values<object>();
            var comparer = Scope.GetComparerOrDefault(Comparer);
            return (Name, first.IsSupersetOf(second, comparer));
        }
    }
}