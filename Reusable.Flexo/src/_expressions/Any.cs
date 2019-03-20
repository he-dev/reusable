using System.Collections.Generic;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public class Any : PredicateExpression
    {
        public Any() : base(nameof(Any), ExpressionContext.Empty) { }

        public List<object> Values { get; set; } = new List<object>();

        public object Predicate { get; set; } = true;

        protected override CalculateResult<bool> Calculate(IExpressionContext context)
        {
            var values = ExtensionInputOrDefault(ref context, Values);
            var predicate = Constant.FromValueOrDefault(nameof(Predicate), Predicate);

            foreach (var item in values.Enabled())
            {
                var result = item.Invoke(context);
                if (predicate is IConstant)
                {
                    if (EqualityComparer<bool>.Default.Equals(result.Value<bool>(), predicate.Value<bool>()))
                    {
                        return (true, result.Context);
                    }
                }
                else
                {
                    var predicateResult = predicate.Invoke(context.Set(Item.For<IExtensionContext>(), x => x.Input, result));
                    if (EqualityComparer<bool>.Default.Equals(predicateResult.Value<bool>(), true))
                    {
                        return (true, result.Context);
                    }
                }
            }

            return (false, context);
        }
    }
}