using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class Contains : Expression, IExtension<List<object>>
    {
        public Contains() : base(nameof(Contains)) { }

        public List<object> Collection { get; set; } = new List<object>();

        public object Value { get; set; }

        public IExpression Comparer { get; set; }

        protected override IExpression InvokeCore(IExpressionContext context)
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

            var collection = context.Input().ValueOrDefault<List<IExpression>>() ?? Collection.Select(Constant.FromValueOrDefault("CollectionItem"));

            var itemContexts =
                from item in collection
                select context
                    .Set(Item.For<IContainsContext>(), x => x.Value, value)
                    .Set(Item.For<IContainsContext>(), x => x.Item, item);

            var contains = itemContexts.Any(itemContext => comparer.Invoke(itemContext).Value<bool>());
            return Constant.FromValue(Name, contains);
        }
    }

    public interface IContainsContext
    {
        object Item { get; }

        object Value { get; }
    }
}