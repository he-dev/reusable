using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public class Collection : Expression<List<object>>
    {
        [JsonConstructor]
        public Collection(SoftString name) : base(name ?? nameof(Collection)) { }

        public List<IExpression> Values { get; set; }

        protected override Constant<List<object>> InvokeCore(IExpressionContext context)
        {
            return (Name, Values.Enabled().Select(e => e.Invoke(context).Value).ToList(), context);
        }
    }
}