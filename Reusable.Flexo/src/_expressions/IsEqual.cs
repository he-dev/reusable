using System;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class IsEqual : ValueExtension<bool>
    {
        public IsEqual(ILogger<IsEqual> logger) : base(logger, nameof(IsEqual)) { }

        [JsonProperty("Left")]
        public override IExpression This { get; set; }

        [JsonRequired]
        public IExpression Value { get; set; }

        protected override Constant<bool> InvokeCore(IExpression @this)
        {
            var value = Value.Invoke().Value;
            return (Name, @this.Invoke().Value<object>().Equals(value));
        }
    }
}