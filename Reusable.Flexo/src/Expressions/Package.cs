using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class Package : Expression<object>
    {
        public Package() : base(default) { }

        [JsonRequired]
        public IExpression Body { get; set; } = default!;

        protected override IConstant ComputeConstant(IImmutableContainer context)
        {
            return Body.Invoke(context);
        }
    }
}