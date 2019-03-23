using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public class Any : PredicateExpression, IExtension<List<IExpression>>
    {
        public Any() : base(nameof(Any)) { }

        public List<IExpression> Values { get; set; } = new List<IExpression>();

        public IExpression Predicate { get; set; } //= (IExpression)Constant.True;

        protected override Constant<bool> InvokeCore(IExpressionContext context)
        {
            var values = ExtensionInputOrDefault(ref context, Values);
            var predicate = Predicate ?? Constant.True;

            foreach (var item in values.Enabled())
            {
                var itemResult = item.Invoke(context);
                if (predicate is IConstant)
                {
                    if (EqualityComparer<bool>.Default.Equals((bool)itemResult.Value, predicate.Value<bool>()))
                    {
                        return (Name, true, itemResult.Context);
                    }
                }
                else
                {
                    var predicateResult = (Constant<bool>)predicate.Invoke(context.Set(Item.For<IExtensionContext>(), x => x.Input, itemResult));
                    if (EqualityComparer<bool>.Default.Equals(predicateResult.Value, true))
                    {
                        return (Name, true, itemResult.Context);
                    }
                }
            }

            return (Name, false, context);
        }
    }
}