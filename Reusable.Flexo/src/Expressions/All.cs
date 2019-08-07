using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class All : CollectionExtension<bool>
    {
        public All(ILogger<All> logger) : base(logger, nameof(All)) { }

        public IEnumerable<IExpression> Values { get => ThisInner ?? ThisOuter; set => ThisInner = value; }

        public IExpression Predicate { get; set; }

        protected override Constant<bool> InvokeCore()
        {
            var predicate = (Predicate ?? Constant.FromValue(nameof(Predicate), true)).Invoke();
            foreach (var item in Values)
            {
                var current = item.Invoke();
                using (BeginScope(ctx => ctx.SetItem(ExpressionContext.ThisOuter, current)))
                {
                    if (!EqualityComparer<bool>.Default.Equals(current.Value<bool>(), predicate.Value<bool>()))
                    {
                        return (Name, false);
                    }
                }
            }

            return (Name, true);
        }
    }
}