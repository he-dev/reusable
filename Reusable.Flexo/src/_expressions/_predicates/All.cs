using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class All : PredicateExpression, IExtension<IEnumerable<object>>
    {
        public All(ILogger<All> logger) : base(logger, nameof(All)) { }

        [This]
        public List<IExpression> Values { get; set; }

        public IExpression Predicate { get; set; }

        protected override Constant<bool> InvokeCore(IImmutableSession context)
        {
            var @this = context.PopThis<IEnumerable<IExpression>>();

            var predicate = (Predicate ?? Constant.True).Invoke(context);
            var last = default(IConstant);
            foreach (var item in @this.Enabled())
            {
                last = item.Invoke(last?.Context ?? context);
                if (!EqualityComparer<bool>.Default.Equals(last.Value<bool>(), predicate.Value<bool>()))
                {
                    return (Name, false, context);
                }
            }

            return (Name, true, last?.Context ?? context);
        }
    }
}