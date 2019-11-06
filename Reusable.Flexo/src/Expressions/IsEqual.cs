using System;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class IsEqual : ScalarExtension<bool>
    {
        public IsEqual(ILogger<IsEqual> logger) : base(logger, nameof(IsEqual)) { }

        public IExpression Left { get => ThisInner; set => ThisInner = value; }

        [JsonRequired]
        public IExpression Value { get; set; }

        protected override bool InvokeAsValue(IImmutableContainer context)
        {
            var value =  Value.Invoke(context).Value;
            return This(context).Invoke(context).Value<object>().Equals(value);
        }
    }
}