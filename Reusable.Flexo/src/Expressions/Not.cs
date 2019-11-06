using System.Collections.Generic;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;
using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    //[Alias("!")]
    public class Not : ScalarExtension<bool>
    {
        public Not() : base(default, nameof(Not)) { }

        public IExpression Value { get => ThisInner; set => ThisInner = value; }

        protected override bool InvokeAsValue(IImmutableContainer context)
        {
            return !This(context).Invoke(context).Value<bool>();
        }
    }
}