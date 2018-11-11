using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Gems.Flexo.Abstractions;
using Gems.Flexo.Extensions;

namespace Gems.Flexo.Expressions
{
    public class Block : Expression, IQuantifier
    {
        public IEnumerable<IAsyncExpression> Expressions { get; set; }

        public override async Task<IAsyncExpression> InvokeAsync(IExpressionContext context, CancellationToken cancellationToken)
        {
            var results = new List<IAsyncExpression>();

            foreach (var expression in Expressions.Enabled())
            {
                results.Add(
                    await expression
                        .ValidateInParameters(context)
                        .InvokeAsync(context, cancellationToken));                
            }

            return new Constant(nameof(Block), results);
        }
    }
}