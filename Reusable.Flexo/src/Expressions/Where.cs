using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    public class Where : CollectionExtension<IEnumerable<IConstant>>, IFilter
    {
        public Where() : base(default) { }

        public IEnumerable<IExpression> Values
        {
            get => Arg;
            set => Arg = value;
        }

        [JsonProperty(Filter.Properties.Predicate)]
        public IExpression Matcher { get; set; }

        protected override IEnumerable<IConstant> ComputeValue(IImmutableContainer context)
        {
            var query =
                from item in GetArg(context).Invoke(context)
                where this.Equal(context, item)
                select item;

            return query.ToList();
        }
    }
}