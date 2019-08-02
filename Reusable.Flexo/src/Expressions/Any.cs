using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;
using Reusable.Quickey;

namespace Reusable.Flexo
{
    public class Any : CollectionExtension<bool>
    {
        public Any(ILogger<Any> logger) : base(logger, nameof(Any)) { }

        public IEnumerable<IExpression> Values { get => ThisInner ?? ThisOuter; set => ThisInner = value; }

        public IExpression Predicate { get; set; }

        protected override Constant<bool> InvokeCore()
        {
            foreach (var item in Values)
            {
                var current = item.Invoke();
                using (BeginScope(ctx => ctx.SetItem(ExpressionContext.ThisOuter, current)))
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