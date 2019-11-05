using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;
using Reusable.Quickey;

namespace Reusable.Flexo
{
    public class ForEach : CollectionExtension<object>
    {
        public ForEach([NotNull] ILogger<ForEach> logger) : base(logger, nameof(ForEach)) { }

        public IEnumerable<IExpression> Values { get => This; set => This = value; }

        public IEnumerable<IExpression> Body { get; set; }

        protected override Constant<object> InvokeCore(IImmutableContainer context)
        {
            foreach (var item in Values.Enabled())
            {
                using (BeginScope(ctx => ctx.SetItem(ExpressionContext.Item, item)))
                {
                    foreach (var expression in Body.Enabled())
                    {
                        expression.Invoke(TODO);
                    }
                }
            }

            return (Name, default(object));
        }
    }
}