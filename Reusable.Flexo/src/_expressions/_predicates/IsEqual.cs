using System;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class IsEqual : PredicateExpression, IExtension<object>
    {
        public IsEqual(ILogger<IsEqual> logger) : base(logger, nameof(IsEqual)) { }

        [JsonRequired]
        //[This]
        public IExpression Value { get; set; }

        protected override Constant<bool> InvokeCore(IImmutableSession context)
        {
            var @this = context.PopThis().Invoke(context).Value<object>();
            
            //if (context.TryPopExtensionInput(out object input))
            {
                var value = Value.Invoke(context).Value;
                return (Name, @this.Equals(value), context);
            }
            //else
            {
              //  throw new InvalidOperationException($"{Name.ToString()} can be used only as an extension.");
            }
        }
    }
}