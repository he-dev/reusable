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

        public override IExpression Invoke(IExpressionContext context)
        {
            var value = Value.Invoke(context).Value<object>();

            var comparer = Comparer ?? new ObjectEqual();

            foreach (var item in Collection)
            {
                var ctx =
                    context
                        .Set(Item.For<IEqualityContext>(), x => x.Left, Constant.FromValue("Left", item))
                        .Set(Item.For<IEqualityContext>(), x => x.Right, value);

                if (comparer.Invoke(ctx).Value<bool>())
                {
                    return Constant.FromValue(Name, true);
                }
            }

            return Constant.FromValue(Name, false);
        }
    }

    public interface IEqualityContext
    {
        IExpression Left { get; }

        IExpression Right { get; }
    }

    //public abstract class Equality : Expression
}