using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    /// <summary>
    /// Invokes each expression and flows the context from one to the other.
    /// </summary>
    [UsedImplicitly]
    [PublicAPI]
    public class Select : Expression<List<IExpression>>, IExtension<IEnumerable<IExpression>>
    {
        public Select(string name) : base(name, ExpressionContext.Empty) { }

        public Select() : this(nameof(Item)) { }

        public IEnumerable<IExpression> Values { get; set; }

        [JsonRequired]
        public IExpression Selector { get; set; }

        protected override ExpressionResult<List<IExpression>> InvokeCore(IExpressionContext context)
        {
            var select =
                ExtensionInputOrDefault(ref context, Values)
                    .Aggregate((Previous: default(IExpressionResult), Expressions: Enumerable.Empty<IExpression>()), (acc, next) =>
                    {
                        var result = next.Invoke(acc.Previous?.Context ?? context);
                        return (result, acc.Expressions.Append(result));
                    });

            return default;
        }

        // private class Scope : Expression
        // {
        //     private readonly IExpression _expression;
        //
        //     public Scope(string name, IExpressionContext context, IExpression expression) : base(name, context)
        //     {
        //         _expression = expression;
        //     }
        //
        //     public override IExpression Invoke(IExpressionContext context)
        //     {
        //         return _expression;
        //     }
        // }
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
//
//    public class Ref : Expression
//    {
//        public Ref(string name) : base(name) { }
//
//        public Ref(string name, IExpressionContext context) : base(name, context) { }
//        
//        public override IExpression Invoke(IExpressionContext context)
//        {
//            throw new NotImplementedException();
//        }
//    }
}