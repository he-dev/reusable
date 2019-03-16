using System;
using System.Collections.Generic;
using System.Linq;
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
    public class Block : Expression
    {
        public Block(string name) : base(name) { }

        public Block() : this(nameof(Item)) { }

        [JsonRequired]
        public IEnumerable<IExpression> Expressions { get; set; }

        protected override IExpression InvokeCore(IExpressionContext context)
        {
            //return new Scope(Expression.Name, context, Expression.Invoke(context));

            var results = new List<IExpression>();
            foreach (var expression in Expressions)
            {
                results.Add(expression.Invoke(results.LastOrDefault()?.Context ?? context));
            }

            return Constant.FromValue(nameof(Block), (results, results.LastOrDefault()?.Context ?? context));
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