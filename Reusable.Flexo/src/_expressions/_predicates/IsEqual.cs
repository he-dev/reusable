using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reusable.OmniLog.Abstractions;
using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    public class IsEqual : PredicateExpression, IExtension<object>
    {
        public IsEqual(ILogger<IsEqual> logger) : base(logger, nameof(IsEqual)) { }

        [JsonRequired]
        public IExpression Value { get; set; }

        protected override Constant<bool> InvokeCore(IExpressionContext context)
        {
            if (context.TryPopExtensionInput(out object input))
            {
                var value = Value.Invoke(context).Value;
                return (Name, input.Equals(value), context);
            }
            else
            {
                throw new InvalidOperationException($"{Name.ToString()} can be used only as an extension.");
            }
        }
    }
}