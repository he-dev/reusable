using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;

namespace Reusable.Flexo
{
    /// <summary>
    /// Invokes each expression and flows the context from one to the other.
    /// </summary>
    [UsedImplicitly]
    [PublicAPI]
    public class Select : Expression<List<object>>, IExtension<IEnumerable<object>>
    {
        public Select(string name) : base(name ?? nameof(Select)) { }

        internal Select() : this(nameof(Select)) { }

        public IEnumerable<IExpression> Values { get; set; }

        [JsonRequired]
        public IExpression Selector { get; set; }

        protected override Constant<List<object>> InvokeCore(IImmutableSession context)
        {
            if (context.TryPopExtensionInput(out IEnumerable<object> input))
            {
                var values = input.Select(x => Selector.Invoke(context.PushExtensionInput(x))).Values<object>().ToList();
                return (Name, values, context);
            }
            else
            {
                var values = Values.Enabled().Invoke(context).Values<object>().ToList();
                return (Name, values, context);
            }
        }
    }


//    public class FirstOrDefault : Expression
//    {
//        [JsonConstructor]
//        public FirstOrDefault() : base(nameof(FirstOrDefault)) { }
//
//        public FirstOrDefault(string name, IExpressionContext context) : base(name, context) { }
//
//        public IExpression Predicate { get; set; }
//
//        public IEnumerable<IDictionary<string, IExpression>> Lookup { get; set; }
//
//        public override IExpression Invoke(IExpressionContext context)
//        {
//            foreach (var item in Lookup)
//            {
//                //if(Predicate)
//            }
//
//            return Constant.Null;
//        }
//    }
}