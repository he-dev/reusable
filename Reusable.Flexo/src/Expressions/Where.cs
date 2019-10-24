using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;
using Reusable.Quickey;

namespace Reusable.Flexo
{
    public class Where : CollectionExtension<IEnumerable<IExpression>>
    {
        public Where([NotNull] ILogger<Where> logger) : base(logger, nameof(Where)) { }

        public IEnumerable<IExpression> Values { get => This; set => This = value; }

        public IExpression Predicate { get; set; }

        protected override Constant<IEnumerable<IExpression>> InvokeCore()
        {
            var result = Values.Enabled().Where(item =>
            {
                using (BeginScope(ctx => ctx.SetItem(ExpressionContext.ThisOuter, item)))
                {
                    return Predicate.Invoke().Value<bool>();
                }
            }).ToList();

            return (Name, result);
        }
    }
}