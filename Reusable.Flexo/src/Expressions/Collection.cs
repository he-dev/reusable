using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reusable.Data;

namespace Reusable.Flexo
{
    public class Collection : Expression<IEnumerable<IExpression>>
    {
        [JsonConstructor]
        public Collection(SoftString name) : base(EmptyLogger.Instance, name ?? nameof(Collection)) { }

        [JsonRequired]
        public IEnumerable<IExpression> Values { get; set; }

        protected override IEnumerable<IExpression> ComputeValue(IImmutableContainer context)
        {
            return 
                Values
                    .Enabled()
                    .Select((e, i) => Constant.FromValue($"{Name.ToString()}.Items[{i}]", e.Invoke(context).Value))
                    .ToList();
        }
    }
}