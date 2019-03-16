using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class Contains : Expression
    {
        public Contains() : base(nameof(Contains)) { }

        [JsonRequired]
        public List<object> Collection { get; set; } = new List<object>();

        [JsonRequired]
        public IExpression Value { get; set; }

        public IExpression Comparer { get; set; }

        protected override IExpression InvokeCore(IExpressionContext context)
        {
            var value = Value.Invoke(context).Value<object>();

            var comparer = Comparer ?? new ObjectEqual
            {
                Left = new GetContextItem
                {
                    Key = ExpressionContext.CreateKey(Item.For<IContainsContext>(), x => x.Value)
                },
                Right = new GetContextItem
                {
                    Key = ExpressionContext.CreateKey(Item.For<IContainsContext>(), x => x.Item)
                }
            };

            foreach (var item in Collection)
            {
                var itemContext =
                    context
                        .Set(Item.For<IContainsContext>(), x => x.Value, value)
                        .Set(Item.For<IContainsContext>(), x => x.Item, Constant.FromValue("CollectionItem", item));
                
                if (comparer.Invoke(itemContext).Value<bool>())
                {
                    return Constant.FromValue(Name, true);
                }
            }

            return Constant.FromValue(Name, false);
        }
    }

    public interface IContainsContext
    {
        object Item { get; }

        object Value { get; }
    }
}