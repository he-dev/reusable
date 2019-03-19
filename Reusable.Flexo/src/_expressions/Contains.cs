using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class Contains : Expression<bool>, IExtension<List<object>>
    {
        public Contains() : base(nameof(Contains), ExpressionContext.Empty) { }

        public List<object> Values { get; set; } = new List<object>();

        public object Value { get; set; }

        public IExpression Comparer { get; set; }

        protected override CalculateResult<bool> Calculate(IExpressionContext context)
        {
            var value = Constant.FromValueOrDefault("Value")(Value).Invoke(context);

            // For convenience ObjectEqual is the default comparer.
            var comparer = Comparer ?? new ObjectEqual();

            if (comparer is IExpressionEqualityComparer equalityComparer)
            {
                // When comparer does not specify otherwise assume Left is Value and Right is Item.

                if (equalityComparer.Left is null)
                {
                    equalityComparer.Left = new GetContextItem
                    {
                        Key = ExpressionContext.CreateKey(Item.For<IContainsContext>(), x => x.Value)
                    };
                }

                if (equalityComparer.Right is null)
                {
                    equalityComparer.Right = new GetContextItem
                    {
                        Key = ExpressionContext.CreateKey(Item.For<IContainsContext>(), x => x.Item)
                    };
                }
            }

            var collection = ExtensionInputOrDefault(ref context, Values);

            var itemContexts =
                from item in collection
                select
                    context
                        .Set(Item.For<IContainsContext>(), x => x.Value, value)
                        .Set(Item.For<IContainsContext>(), x => x.Item, item);

            var contains = itemContexts.Any(itemContext => comparer.Invoke(itemContext).Value<bool>());
            return (contains, context);
        }
    }        

    public interface IContainsContext
    {
        object Item { get; }

        object Value { get; }
    }
}