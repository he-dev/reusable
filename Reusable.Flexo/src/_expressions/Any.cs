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

        protected override Constant<bool> InvokeCore(IEnumerable<IExpression> @this)
        {
            foreach (var item in @this)
            {
                var current = item.Invoke();
                using (BeginScope(ctx => ctx.SetItem(From<IExpressionMeta>.Select(m => m.This), current)))
                {
                    var predicate = (Predicate ?? Constant.True);
                    if (predicate is IConstant)
                    {
                        if (EqualityComparer<bool>.Default.Equals(current.Value<bool>(), predicate.Invoke().Value<bool>()))
                        {
                            return (Name, true);
                        }
                    }
                    else
                    {
                        if (EqualityComparer<bool>.Default.Equals(predicate.Invoke().Value<bool>(), true))
                        {
                            return (Name, true);
                        }
                    }
                }
            }

            return (Name, false);
        }
    }
}