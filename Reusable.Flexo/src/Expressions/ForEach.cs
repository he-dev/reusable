using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class ForEach : Extension<object, object>
    {
        public ForEach() : base(default) { }

        public IEnumerable<IExpression>? Values
        {
            set => Arg = value;
        }

        [JsonRequired]
        public IExpression Body { get; set; } = default!;

        protected override IConstant ComputeConstant(IImmutableContainer context)
        {
            var query =
                from value in GetArg(context)
                select Body.Invoke(context.BeginScopeWithArg($"{nameof(ForEach)}.Item", value));

            return query.LastOrDefault() ?? throw new InvalidOperationException($"{nameof(ForEach)} '{Id}' must have at least one value.");
        }
    }
}