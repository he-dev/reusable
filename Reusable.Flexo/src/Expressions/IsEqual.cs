using System;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class IsEqual : ScalarExtension<bool>
    {
        public IsEqual(ILogger<IsEqual> logger) : base(logger, nameof(IsEqual)) { }

        public IExpression Left { get => ThisInner ?? ThisOuter; set => ThisInner = value; }

        [JsonRequired]
        public IExpression Value { get; set; }

        protected override Constant<bool> InvokeCore(IImmutableContainer context)
        {
            var value = Value.Invoke(TODO).Value;
            return (Name, Left.Invoke(TODO).Value<object>().Equals(value));
        }
    }
}