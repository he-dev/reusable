using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class Block : Expression
    {
        public Block() : base(default)
        {
            Body = new[] { new Throw { Exception = "EmptyBlockBody", Message = $"{nameof(Block)} '{Id}' must have at least one element." } };
        }

        public IEnumerable<IExpression> Body { get; set; }

        protected override IConstant ComputeConstant(IImmutableContainer context)
        {
            return Body.Select(e => e.Invoke(context)).ToList() switch
            {
                {} results when results.Any() => results.Last(),
                _ => throw DynamicException.Create("EmptyBlockBody", $"{nameof(Block)} '{Id.ToString()}' must have at least one element.")
            };
        }
    }
}