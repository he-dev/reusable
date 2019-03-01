using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Reusable.Flexo
{
    /// <summary>
    /// Wraps expression-context.
    /// </summary>
    public class Block : Expression
    {
        public Block(string name) : base(name)
        { }

        public Block() : this(nameof(Item))
        { }

        public IEnumerable<IExpression> Expressions { get; set; }

        public override IExpression Invoke(IExpressionContext context)
        {
            //return new Scope(Expression.Name, context, Expression.Invoke(context));

            var results = new List<IExpression>();
            foreach (var expression in Expressions)
            {
                results.Add(expression.Invoke(results.LastOrDefault()?.Context ?? context));
            }

            return Constant.Create(nameof(Block), InvokeResult.From(results, results.LastOrDefault()?.Context ?? context));
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
}