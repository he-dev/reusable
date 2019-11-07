using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Block : Expression
    {
        public Block() : base(default, nameof(Block)) { }

        public IEnumerable<IExpression> Body { get; set; }

        protected override IConstant ComputeConstant(IImmutableContainer context)
        {
            if (Body is null) throw new InvalidOperationException($"{nameof(Block)} '{Name.ToString()}' {nameof(Body)} must not be null.");

            return Body.Select(item => item.Invoke(context)).ToList() switch
            {
                {} results when results.Any() => results.Last(),
                _ => throw DynamicException.Create("EmptyBlockBody", $"{nameof(Block)} '{Name.ToString()}' must have at least one element.")
            };
        }
    }
}