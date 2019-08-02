using System;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class IsEqual : ValueExpressionExtension<bool>
    {
        public IsEqual(ILogger<IsEqual> logger) : base(logger, nameof(IsEqual)) { }

        public IExpression Left { get => ThisInner ?? ThisOuter; set => ThisInner = value; }

        [JsonRequired]
        public IExpression Value { get; set; }

        protected override Constant<bool> InvokeCore()
        {
            var value = Value.Invoke().Value;
            return (Name, Left.Invoke().Value<object>().Equals(value));
        }
    }
}