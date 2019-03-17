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

        [JsonRequired]
        public object Value { get; set; }

        public IExpression Comparer { get; set; }

        protected override IExpression InvokeCore(IExpressionContext context)
        {
            var value = Value is IExpression expression ? expression : Constant.FromValue("Value", Value);

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

            var collection = 
                context.Input().ValueOrDefault<List<IExpression>>() 
                ?? Collection.Select(x => Constant.FromValue("CollectionItem", x)).Cast<IExpression>();
            
            foreach (var item in collection)
            {
                var itemContext =
                    context
                        .Set(Item.For<IContainsContext>(), x => x.Value, value)
                        .Set(Item.For<IContainsContext>(), x => x.Item, item);
                
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