using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    public class IsSuperset : Extension<object, bool>, IFilter
    {
        public IsSuperset() : base(default) { }

        public IEnumerable<IExpression>? Values
        {
            //get => Arg;
            set => Arg = value;
        }

        [JsonRequired]
        public List<IExpression> Of { get; set; } = default!;

        [JsonProperty(Filter.Properties.Comparer)]
        public IExpression? Matcher { get; set; }

        protected override bool ComputeSingle(IImmutableContainer context)
        {
            var first = GetArg(context);
            var second = Of.Invoke(context).SelectMany(c => c);
            var comparer = this.GetEqualityComparer(context);
            return first.IsSupersetOf(second, comparer);
        }
    }
}