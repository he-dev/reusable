using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    /// <summary>
    /// Invokes each expression and flows the context from one to the other.
    /// </summary>
    [UsedImplicitly]
    [PublicAPI]
    public class Select : Extension<object, object>
    {
        public Select() : base(default) { }

        public IEnumerable<IExpression>? Values
        {
            set => Arg = value;
        }

        [JsonRequired]
        public IExpression Selector { get; set; } = default!;

        protected override IEnumerable<object> ComputeMany(IImmutableContainer context)
        {
            return 
                from item in GetArg(context)
                from result in Selector.Invoke(context.BeginScopeWithArg(Constant.Single($"{nameof(Select)}.Item", item))).Cast<object>()
                select result;
        }
    }
}