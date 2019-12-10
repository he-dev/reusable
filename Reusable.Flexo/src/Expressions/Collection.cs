using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    public class Collection : Expression<object>
    {
        [JsonConstructor]
        public Collection() : base(default) { }

        [JsonRequired]
        public IEnumerable<IExpression> Values { get; set; } = null!;

        protected override IEnumerable<object> ComputeValues(IImmutableContainer context)
        {
            return Values.Enabled().SelectMany(e => e.Invoke(context));
        }
    }
}