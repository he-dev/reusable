using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public class Collection : Expression
    {
        [JsonConstructor]
        public Collection(SoftString name) : base(name, ExpressionContext.Empty) { }

        public List<object> Values { get; set; }

        protected override IExpression InvokeCore(IExpressionContext context)
        {
            var values = Values.Select(Constant.FromValueOrDefault("Item"));
            return Constant.FromValue(Name, values);
        }
    }

    public interface ICollectionContext
    {
        object Item { get; }
    }
}