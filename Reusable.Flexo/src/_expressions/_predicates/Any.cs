using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Any : CollectionExtension<bool>
    {
        public Any(ILogger<Any> logger) : base(logger, nameof(Any)) { }

        [JsonProperty("Values")]
        public override IEnumerable<IExpression> This { get; set; }

        public IExpression Predicate { get; set; }

        protected override Constant<bool> InvokeCore(IImmutableSession context, IEnumerable<IExpression> @this)
        {
            var last = default(IConstant);
            foreach (var item in @this)
            {
                var value = item.Invoke(context);
                context.PushThis(value);
                var predicate = (Predicate ?? Constant.True);

                if (predicate is IConstant)
                {
                    last = value;
                    if (EqualityComparer<bool>.Default.Equals(last.Value<bool>(), predicate.Invoke(context).Value<bool>()))
                    {
                        return (Name, true, last.Context);
                    }
                }
                else
                {
                    last = value; //.Invoke(predicate.Context);
                    if (EqualityComparer<bool>.Default.Equals(predicate.Invoke(context).Value<bool>(), true))
                        //if (EqualityComparer<bool>.Default.Equals(predicate.Value<bool>(), true))
                    {
                        return (Name, true, last.Context);
                    }
                }
            }

            return (Name, false, context);
        }
    }
}