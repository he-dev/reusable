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

        public IEnumerable<IExpression> Values { get => This; set => This = value; }

        public IExpression Predicate { get; set; }

        protected override Constant<bool> InvokeCore(IImmutableContainer context)
        {
            var predicate = (Predicate ?? Constant.FromValue(nameof(Predicate), true)).Invoke(context);
            foreach (var item in Values.Enabled())
            {
                var current = item.Invoke(context);
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