using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Data;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class ForEach : CollectionExtension<object>
    {
        public ForEach() : base(default) { }

        public IEnumerable<IExpression> Source { get => Arg; set => Arg = value; }

        public IExpression Body { get; set; }

        protected override object ComputeValue(IImmutableContainer context)
        {
            var query =
                from arg in GetArg(context)
                select Body.Invoke(context.BeginScopeWithArg(arg));

            var results = query.ToList();
            return results.LastOrDefault()?.Value;
        }
    }
}