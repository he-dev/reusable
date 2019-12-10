using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    [UsedImplicitly]
    [PublicAPI]
    public class Throw : Expression<IExpression>
    {
        public Throw() : base(default) { }

        public string? Exception { get; set; } = default!;

        [JsonRequired]
        public string Message { get; set; } = default!;

        protected override IEnumerable<IExpression> ComputeMany(IImmutableContainer context)
        {
            throw DynamicException.Create(Exception ?? Id.ToString(), Message);
        }
    }
}