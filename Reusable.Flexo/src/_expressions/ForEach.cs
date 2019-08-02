using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;
using Reusable.Quickey;

namespace Reusable.Flexo
{
    public class ForEach : CollectionExpressionExtension<object>
    {
        public ForEach([NotNull] ILogger<ForEach> logger) : base(logger, nameof(ForEach)) { }

        public IEnumerable<IExpression> Values { get => ThisInner ?? ThisOuter; set => ThisInner = value; }

        public IEnumerable<IExpression> Body { get; set; }

        protected override Constant<object> InvokeCore()
        {
            foreach (var item in Values)
            {
                using (BeginScope(ctx => ctx.SetItem(ExpressionContext.Item, item)))
                {
                    foreach (var expression in Body)
                    {
                        expression.Invoke();
                    }
                }
            }

            return (Name, default(object));
        }
    }
}