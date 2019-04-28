using System.Collections.Generic;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Any : PredicateExpression, IExtension<List<object>>
    {
        public Any(ILogger<Any> logger) : base(logger, nameof(Any)) { }

        [This]
        public List<IExpression> Values { get; set; } = new List<IExpression>();

        public IExpression Predicate { get; set; }

        protected override Constant<bool> InvokeCore(IImmutableSession context)
        {
            if (context.TryPopExtensionInput(out IEnumerable<object> input))
            {
                foreach (var item in input)
                {
                    context.PushExtensionInput(item);
                    var predicateResult = (Predicate ?? Constant.True).Invoke(context).Value<bool>();
                    if (EqualityComparer<bool>.Default.Equals(predicateResult, true))
                    {
                        return (Name, true, context);
                    }
                }
            }
            else
            {
                var predicate = (Predicate ?? Constant.True).Invoke(context);
                var last = default(IConstant);
                foreach (var item in Values.Enabled())
                {
                    last = item.Invoke(predicate.Context);
                    if (EqualityComparer<bool>.Default.Equals(last.Value<bool>(), predicate.Value<bool>()))
                    {
                        return (Name, true, last.Context);
                    }
                }
            }

            return (Name, false, context);
        }
    }
}