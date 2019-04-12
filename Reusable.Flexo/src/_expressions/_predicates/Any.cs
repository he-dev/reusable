using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Any : PredicateExpression, IExtension<List<object>>
    {
        public Any(ILogger<Any> logger) : base(logger, nameof(Any)) { }

        public List<IExpression> Values { get; set; } = new List<IExpression>();

        public IExpression Predicate { get; set; }

        protected override Constant<bool> InvokeCore(IExpressionContext context)
        {
            if (context.TryPopExtensionInput(out IEnumerable<object> input))
            {
                foreach (var item in input)
                {
                    context.Get(Item.For<IExtensionContext>(), x => x.Inputs).Push(item);
                    var predicateResult = (Predicate ?? Constant.True).Invoke(context).Value<bool>();
                    if (EqualityComparer<bool>.Default.Equals(predicateResult, true))
                    {
                        return (Name, true, context);
                    }
                }
            }
            else
            {
                var predicate = (Predicate ?? Constant.True).Invoke(context).Value<bool>();
                var last = default(IConstant);
                foreach (var item in Values)
                {
                    last = item.Invoke(context);
                    if (EqualityComparer<bool>.Default.Equals(last.Value<bool>(), predicate))
                    {
                        return (Name, true, last?.Context ?? context);
                    }
                }
            }

            return (Name, false, context);
        }
    }
}