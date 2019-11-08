using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Exceptionize;

namespace Reusable.Flexo
{
    public class Block : Expression
    {
        public Block() : base(default) { }

        public IEnumerable<IExpression> Body { get; set; }

        protected override IConstant ComputeConstant(IImmutableContainer context)
        {
            if (Body is null) throw new InvalidOperationException($"{nameof(Block)} '{Id.ToString()}' {nameof(Body)} must not be null.");

            return Body.Select(e => e.Invoke(context)).ToList() switch
            {
                {} results when results.Any() => results.Last(),
                _ => throw DynamicException.Create("EmptyBlockBody", $"{nameof(Block)} '{Id.ToString()}' must have at least one element.")
            };
        }
    }
}